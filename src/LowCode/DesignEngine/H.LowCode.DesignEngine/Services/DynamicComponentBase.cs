using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Ids;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace H.LowCode.DesignEngine;

public abstract class DynamicComponentBase : LowCodeDynamicComponentBase
{
    protected virtual RenderFragment RenderComponent(ComponentPartsSchema component)
        => builder =>
    {
        if (component == null || component.Fragment == null)
            throw new NullReferenceException($"{nameof(component)} or {nameof(component.Fragment)} is null");

        int index = 0;
        RenderComponentRecursive(component.Id, component.IsSupportDataSource,
            component, component.DataSource, component.Fragment, builder, index);
    };

    private void RenderComponentRecursive(
        string componentId, bool isSupportDataSource,
        ComponentPartsSchema component,
        ComponentPartsDataSourceSchema dataSource,
        ComponentPartsFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        //TypeName 为空时，使用 DefaultTypeName
        if (string.IsNullOrEmpty(componentFragment.TypeName))
            componentFragment.TypeName = componentFragment.DefaultTypeName;

        if (string.IsNullOrEmpty(componentFragment.TypeName))
            throw new NullReferenceException($"componentId={componentId}, {nameof(componentFragment.TypeName)}");

        // 原生 html 元素（frag.dt = "html:{tag}"）：直接渲染原生标签
        if (NativeHtmlElement.IsNativeHtml(componentFragment.TypeName))
        {
            RenderNativeHtmlFragment(componentId, component, componentFragment, builder,
                string.Empty, componentFragment);
            return;
        }

        Type componentType = ResolveComponentType(componentFragment.TypeName);
        if (componentType == null)
            return; // 无法解析的类型跳过渲染，避免整页崩溃

        builder.OpenComponent(index++, componentType);

        //渲染属性
        RenderComponentAttributes(builder, index, componentId, componentType,
            componentFragment.Attributes);

        //渲染 ChildContent
        if (isSupportDataSource)
        {
            RenderDataSource(componentId, component, dataSource, builder, index);
        }
        else if (componentFragment.HasChildFragment)
        {
            RenderChildFragments(componentId, component, componentFragment, builder, index);
        }
        else if (string.IsNullOrWhiteSpace(componentFragment.Content) == false)
        {
            RenderContent(componentId, component, componentFragment, builder, index);
        }

        builder.CloseComponent();
    }

    #region 渲染原生 html 元素
    /// <summary>
    /// 渲染原生 html 元素 Fragment（frag.dt = "html:{tag}"）。
    /// 属性约定：attrn 为 html 属性名（小写）；"class" 多条自动合并；"content" 表示文本内容；
    /// 嵌套子元素属性用 "childs.{i}..." 路径定位。
    /// </summary>
    private void RenderNativeHtmlFragment(string componentId,
        ComponentPartsSchema component,
        ComponentPartsFragmentSchema fragment,
        RenderTreeBuilder builder,
        string path, ComponentPartsFragmentSchema rootFragment,
        (string? OptionValue, string? OptionLabel)? option = null)
    {
        var tagName = NativeHtmlElement.GetTagName(fragment.TypeName);
        if (string.IsNullOrEmpty(tagName))
            return;

        builder.OpenElement(0, tagName);

        var contentOverride = RenderNativeHtmlAttributes(componentId, fragment, rootFragment,
            path, builder, option);

        // 内容：$(DraggableContainer) 标记 → 注入可拖拽子容器；否则渲染文本内容
        var content = contentOverride ?? fragment.Content;
        if (string.Equals(content, NativeHtmlElement.DraggableContainerToken, StringComparison.OrdinalIgnoreCase))
        {
            var containerKey = $"container-{component.Id}-{(string.IsNullOrEmpty(path) ? "root" : path)}";
            var (containerComponent, needAdd) = RenderContainerComponent(component, containerKey);
            if (needAdd)
            {
                builder.OpenComponent<DraggableContainer>(0);
                builder.AddAttribute(1, "ContainerComponent", containerComponent);
                builder.CloseComponent();
            }
        }
        else if (string.IsNullOrWhiteSpace(content) == false)
        {
            var text = content;
            if (option.HasValue)
                text = NativeHtmlElement.SubstituteOptionToken(text, option.Value.OptionValue, option.Value.OptionLabel);
            builder.AddContent(0, text);
        }

        // 递归渲染子元素
        if (fragment.HasChildFragment)
        {
            for (var i = 0; i < fragment.ChildFragments.Length; i++)
            {
                RenderNativeHtmlChildFragment(componentId, component, fragment.ChildFragments[i],
                    builder, NativeHtmlElement.ChildPath(path, i), rootFragment, option);
            }
        }

        builder.CloseElement();
    }

    /// <summary>
    /// 渲染子 Fragment：原生 html 走元素渲染，.NET 类型走组件渲染
    /// </summary>
    private void RenderNativeHtmlChildFragment(string componentId,
        ComponentPartsSchema component,
        ComponentPartsFragmentSchema fragment,
        RenderTreeBuilder builder,
        string path, ComponentPartsFragmentSchema rootFragment,
        (string? OptionValue, string? OptionLabel)? option = null)
    {
        if (NativeHtmlElement.IsNativeHtml(fragment.TypeName))
        {
            RenderNativeHtmlFragment(componentId, component, fragment, builder, path, rootFragment, option);
            return;
        }

        RenderComponentRecursive(componentId, false, component, null, fragment, builder, 0);
    }

    /// <summary>
    /// 渲染原生 html 元素属性，返回 content 覆盖值（无则为 null）
    /// </summary>
    private string? RenderNativeHtmlAttributes(string componentId,
        ComponentPartsFragmentSchema fragment,
        ComponentPartsFragmentSchema rootFragment,
        string path, RenderTreeBuilder builder,
        (string? OptionValue, string? OptionLabel)? option)
    {
        string? contentOverride = null;
        var classes = new List<string>();
        var isRoot = ReferenceEquals(fragment, rootFragment);

        foreach (var attr in fragment.Attributes)
        {
            if (string.IsNullOrEmpty(attr?.AttributeName))
                continue;

            var (attrPath, attrName) = NativeHtmlElement.ParseAttributePath(attr.AttributeName);

            // 根节点：路径属性在递归到对应子节点时处理，此处跳过
            if (isRoot && attrPath.Length > 0)
                continue;
            // 非根节点：只处理定位到当前路径的属性（自身的静态属性 attrn 无路径前缀）
            if (!isRoot && attrPath.Length > 0 && attrPath != path)
                continue;

            ApplyNativeHtmlAttribute(componentId, attr, attrName, builder,
                classes, ref contentOverride, option);
        }

        // 非根节点：应用根上声明的路径属性（如属性面板编辑的嵌套内容）
        if (!isRoot)
        {
            foreach (var attr in rootFragment.Attributes)
            {
                if (string.IsNullOrEmpty(attr?.AttributeName))
                    continue;

                var (attrPath, attrName) = NativeHtmlElement.ParseAttributePath(attr.AttributeName);
                if (attrPath != path)
                    continue;

                ApplyNativeHtmlAttribute(componentId, attr, attrName, builder,
                    classes, ref contentOverride, option);
            }
        }

        if (classes.Count > 0)
            builder.AddAttribute(0, "class", string.Join(" ", classes));

        return contentOverride;
    }

    private void ApplyNativeHtmlAttribute(string componentId,
        ComponentAttributeFragmentSchema attr, string attrName,
        RenderTreeBuilder builder, List<string> classes,
        ref string? contentOverride,
        (string? OptionValue, string? OptionLabel)? option)
    {
        var value = attr.AttributeValue?.ToString();

        if (option.HasValue)
            value = NativeHtmlElement.SubstituteOptionToken(value, option.Value.OptionValue, option.Value.OptionLabel);

        if (string.Equals(attrName, "class", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(value))
                classes.Add(value);
            return;
        }

        if (string.Equals(attrName, "content", StringComparison.OrdinalIgnoreCase))
        {
            contentOverride = value;
            return;
        }

        if (string.IsNullOrEmpty(attrName))
            return;

        var lowerName = attrName.ToLowerInvariant();
        // maxlength/rows 为 0 表示不限制，跳过渲染
        if ((lowerName == "maxlength" || lowerName == "rows") && value == "0")
            return;


        // 布尔属性：true 渲染、false 省略
        if (string.Equals(attr.AttributeClrType, "System.Boolean", StringComparison.Ordinal))
        {
            if (bool.TryParse(value, out var boolValue) && boolValue)
                builder.AddAttribute(0, lowerName, true);
            return;
        }

        if (value == null)
            return;

        builder.AddAttribute(0, lowerName, value);
    }
    #endregion

    #region 渲染数据源
    private void RenderDataSource(string componentId,
        ComponentPartsSchema component,
        ComponentPartsDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource == null)
            return;

        if (dataSource.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.Option)
        {
            switch (dataSource.DataSourceType)
            {
                case ComponentDataSourceTypeEnum.Fiexd:
                    RenderOptionDataSource(componentId, dataSource, builder, index);
                    break;
                case ComponentDataSourceTypeEnum.SQL:
                    break;
                case ComponentDataSourceTypeEnum.API:
                    break;
                default:
                    break;
            }
        }
        else if (dataSource.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.Table)
        {
            builder.AddAttribute(index++, "DataSource", component.DataSource);
        }
        else if (dataSource.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.List)
        {
            RenderListDataSource(componentId, component, dataSource, builder, index);
        }
    }

    private void RenderOptionDataSource(string componentId,
        ComponentPartsDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource.FiexdOptionDataSource == null
            || dataSource.FiexdOptionDataSource.Count == 0)
            return;

        // 无 DataSourceFragment（Hc 风格的选项组件）时，设计时不预览选项，避免空引用
        if (dataSource.DataSourceFragment == null)
            return;

        // 原生 html 选项 Fragment（如 label > input[radio] + span）：按选项逐个渲染并替换 $(value)/$(label) 占位符
        if (NativeHtmlElement.IsNativeHtml(dataSource.DataSourceFragment.TypeName))
        {
            builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
            {
                foreach (var option in dataSource.FiexdOptionDataSource)
                {
                    RenderNativeHtmlFragment(componentId, null, dataSource.DataSourceFragment,
                        childBuilder, string.Empty, dataSource.DataSourceFragment,
                        (option.Value?.ToString(), option.Label));
                }
            }));
            return;
        }

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            if (string.IsNullOrEmpty(dataSource.DataSourceFragment.TypeName))
                return;

            Type childComponentType = ResolveComponentType(dataSource.DataSourceFragment.TypeName);
            if (childComponentType == null)
                return;

            foreach (var option in dataSource.FiexdOptionDataSource)
            {
                childBuilder.OpenComponent(index++, childComponentType);
                foreach (var fragAttr in dataSource.DataSourceFragment.Attributes)
                {
                    if (string.IsNullOrEmpty(fragAttr.AttributeName))
                        throw new NullReferenceException($"componentId={componentId}, {nameof(fragAttr.AttributeName)} is null");

                    childBuilder.AddAttribute(index++, fragAttr.AttributeName, option.Value);
                }

                childBuilder.AddAttribute(index++, "ChildContent", (RenderFragment)((cb) =>
                {
                    cb.AddContent(index++, option.Label);
                }));

                childBuilder.CloseComponent();
            }
        }));
    }
    #endregion

    #region 渲染子节点
    private void RenderChildFragments(string componentId,
        ComponentPartsSchema component,
        ComponentPartsFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        if (componentFragment.HasChildFragment == false)
            return;

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            foreach (var childFragment in componentFragment.ChildFragments)
            {
                RenderComponentRecursive(componentId, false,
                    component, null, childFragment, childBuilder, index);
            }
        }));
    }
    #endregion

    #region 渲染 List 循环数据源
    private void RenderListDataSource(string componentId,
        ComponentPartsSchema component,
        ComponentPartsDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource.ListDataSource == null)
        {
            // 如果没有配置数据源，渲染空列表
            builder.AddAttribute(index++, "DataSource", new List<object>());
            return;
        }

        // 获取固定数据源（设计时预览）
        var listData = GetListDataSource(dataSource);
        if (listData == null || listData.Count == 0)
        {
            // 使用示例数据
            listData = new List<object>
            {
                new Dictionary<string, object> { { "id", "1" }, { "title", "示例问题 1" } },
                new Dictionary<string, object> { { "id", "2" }, { "title", "示例问题 2" } }
            };
        }

        // 渲染 ItemTemplate
        if (dataSource.DataSourceFragment != null)
        {
            // 原生 html 列表项模板：逐行渲染原生元素树
            if (NativeHtmlElement.IsNativeHtml(dataSource.DataSourceFragment.TypeName))
            {
                builder.AddAttribute(index++, "ChildContent", (RenderFragment<object>)((item) => (childBuilder) =>
                {
                    RenderNativeHtmlFragment(componentId, component, dataSource.DataSourceFragment,
                        childBuilder, string.Empty, dataSource.DataSourceFragment);
                }));

                builder.AddAttribute(index++, "DataSource", listData);
                return;
            }

            builder.AddAttribute(index++, "ChildContent", (RenderFragment<object>)((item) => (childBuilder) =>
            {
                if (string.IsNullOrEmpty(dataSource.DataSourceFragment.TypeName))
                    return;

                Type itemComponentType = ResolveComponentType(dataSource.DataSourceFragment.TypeName);
                if (itemComponentType == null)
                    return;

                childBuilder.OpenComponent(index++, itemComponentType);

                // 渲染属性
                if (dataSource.DataSourceFragment.Attributes != null)
                {
                    foreach (var attr in dataSource.DataSourceFragment.Attributes)
                    {
                        if (string.IsNullOrEmpty(attr.AttributeName))
                            continue;

                        var attrValue = ResolveAttributeValue(attr, item);

                        if (!string.IsNullOrEmpty(attr.AttributeClrType))
                        {
                            var attrType = Type.GetType(attr.AttributeClrType);
                            if (attrType != null)
                            {
                                var realValue = attrValue?.ToString().ConvertToRealType(attrType) ?? attrValue;
                                childBuilder.AddAttribute(index++, attr.AttributeName, realValue);
                            }
                        }
                        else
                        {
                            childBuilder.AddAttribute(index++, attr.AttributeName, attrValue);
                        }
                    }
                }

                // 渲染子组件
                if (dataSource.DataSourceFragment.HasChildFragment)
                {
                    RenderChildFragments(componentId, component, dataSource.DataSourceFragment, childBuilder, index);
                }

                childBuilder.CloseComponent();
            }));
        }

        builder.AddAttribute(index++, "DataSource", listData);
    }

    private IList<object> GetListDataSource(ComponentPartsDataSourceSchema dataSource)
    {
        var listDs = dataSource.ListDataSource;
        if (listDs == null)
            return new List<object>();

        if (listDs.FixedData != null && listDs.FixedData.Count > 0)
        {
            return listDs.FixedData.Cast<object>().ToList();
        }

        return new List<object>();
    }

    private object ResolveAttributeValue(ComponentAttributeFragmentSchema attr, object dataItem)
    {
        if (attr.AttributeValue == null)
            return null;

        var valueStr = attr.AttributeValue.ToString();
        if (string.IsNullOrEmpty(valueStr))
            return null;

        // 支持绑定表达式 $(item.fieldName)
        if (valueStr.StartsWith("$(item.") && valueStr.EndsWith(")"))
        {
            var fieldName = valueStr.Substring(7, valueStr.Length - 8);

            if (dataItem is Dictionary<string, object> dict)
            {
                return dict.ContainsKey(fieldName) ? dict[fieldName] : null;
            }
            else
            {
                var propInfo = dataItem.GetType().GetProperty(fieldName);
                return propInfo?.GetValue(dataItem);
            }
        }

        return attr.AttributeValue;
    }
    #endregion

    #region 渲染 Content
    private void RenderContent(string componentId,
        ComponentPartsSchema component,
        ComponentPartsFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        if (string.IsNullOrWhiteSpace(componentFragment.Content))
            return;

        if (string.Equals(componentFragment.Content, $"$({nameof(DraggableContainer)})",
            StringComparison.OrdinalIgnoreCase))
        {
            //TODO: 此处 containerComponentId 不能保证唯一性, 待优化
            var containerComponentId = $"container-{component.Id}-{componentFragment.DefaultTypeName.GetHashCode()}";
            var (containerComponent, needAdd) = RenderContainerComponent(component, containerComponentId);
            if (needAdd == false)
                return;

            builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.OpenComponent<DraggableContainer>(index++);
                childBuilder.AddAttribute(index++, "ContainerComponent", containerComponent);
                childBuilder.CloseComponent();
            }));
        }
        else
        {
            builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
            {
                childBuilder.AddMarkupContent(index++, componentFragment.Content);
            }));
        }
    }
    #endregion

    #region 渲染组件内的 DraggableContainer
    private (ComponentPartsSchema, bool) RenderContainerComponent(ComponentPartsSchema component, string key, Action<ComponentPartsSchema> action = null)
    {
        var exist = component.Childrens?.Any(t => t.Id == key);
        if (exist.HasValue && exist.Value)
            return (null, false);

        var innerContainerComponent = RenderChildContainerComponent(component, key);

        if (action != null) action(innerContainerComponent);

        component.Childrens.Add(innerContainerComponent);

        return (innerContainerComponent, true);
    }

    private ComponentPartsSchema RenderChildContainerComponent(ComponentPartsSchema component, string key)
    {
        var innerContainerComponent = new ComponentPartsSchema
        {
            Id = key,
            PartsId = ShortIdGenerator.Generate(),
            Refresh = component.Refresh,

            Fragment = new(),
            Style = new() { DefaultStyle = "height:100%; width:100%;" },
            IsHiddenLabel = true,

            IsContainer = true,
            IsInnerContainer = true,
            ParentId = component.Id
        };

        return innerContainerComponent;
    }
    #endregion
}
