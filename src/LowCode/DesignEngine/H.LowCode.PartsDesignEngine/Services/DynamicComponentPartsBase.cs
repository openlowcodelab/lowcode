using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Ids;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace H.LowCode.PartsDesignEngine;

public abstract class DynamicComponentPartsBase : LowCodeDynamicComponentBase
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
            // List 数据源，使用默认示例数据
            var sampleData = new List<object>
            {
                new Dictionary<string, object> { { "id", "1" }, { "title", "示例项 1" } },
                new Dictionary<string, object> { { "id", "2" }, { "title", "示例项 2" } }
            };
            builder.AddAttribute(index++, "DataSource", sampleData);
        }
    }

    private void RenderOptionDataSource(string componentId,
        ComponentPartsDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource.FiexdOptionDataSource == null
            || dataSource.FiexdOptionDataSource.Count == 0)
            return;

        // 无 DataSourceFragment（Hc 风格选项组件）时不渲染选项，避免空引用
        if (dataSource.DataSourceFragment == null)
            return;

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
            ParentId = component.Id,

            // 初始化 Childrens 为空列表，并从父组件复制子组件
            Childrens = component.Childrens != null ? new List<ComponentPartsSchema>(component.Childrens) : new List<ComponentPartsSchema>()
        };

        return innerContainerComponent;
    }
    #endregion
}
