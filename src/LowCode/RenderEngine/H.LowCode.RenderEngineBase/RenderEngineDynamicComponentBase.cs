using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.RenderEngine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace H.LowCode.RenderEngineBase;

public abstract class RenderEngineDynamicComponentBase : LowCodeDynamicComponentBase, IDisposable
{
    [CascadingParameter(Name = "pageCascading")]
    public PageCascadingModel PageCascading { get; set; }

    [Inject]
    protected ListDataOperationManager ListDataManager { get; set; }

    [Inject]
    protected PageFormStateService FormState { get; set; }

    [Inject]
    protected PageComponentRegistry ComponentRegistry { get; set; }

    [Inject]
    protected ITableDataAppService TableDataAppService { get; set; }

    [Inject]
    protected IFormDataAppService FormDataAppService { get; set; }

    // 当前 List 组件 ID，用于事件处理
    protected string CurrentListId { get; set; }

    // 已从数据库加载的列表组件 Id
    private readonly HashSet<string> _loadedListIds = [];
    // 正在加载中的列表组件 Id
    private readonly HashSet<string> _loadingListIds = [];
    private bool _formStateSubscribed;
    private bool _listDataSubscribed;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // 订阅表单状态变化，驱动显隐联动与值回显重新求值
        if (FormState != null && !_formStateSubscribed)
        {
            FormState.OnChange += OnFormStateChanged;
            _formStateSubscribed = true;
        }

        // 订阅列表数据变化（增删行/排序等），驱动列表重新渲染
        if (ListDataManager != null && !_listDataSubscribed)
        {
            ListDataManager.OnChange += OnListDataChanged;
            _listDataSubscribed = true;
        }
    }

    private void OnFormStateChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnListDataChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public virtual void Dispose()
    {
        if (_formStateSubscribed && FormState != null)
        {
            FormState.OnChange -= OnFormStateChanged;
            _formStateSubscribed = false;
        }

        if (_listDataSubscribed && ListDataManager != null)
        {
            ListDataManager.OnChange -= OnListDataChanged;
            _listDataSubscribed = false;
        }
    }

    #region 表达式上下文

    /// <summary>
    /// 创建表达式求值上下文
    /// </summary>
    protected LowCodeExpressionContext CreateExpressionContext(object? dataContext = null)
    {
        object? item = dataContext is ListItemContext ctx ? ctx.Item : dataContext;
        return new LowCodeExpressionContext
        {
            Item = item,
            FormState = FormState,
            QueryProvider = key => GetQueryValue(key)
        };
    }

    /// <summary>
    /// 计算组件的表单值状态 key
    /// </summary>
    /// <remarks>普通组件为 Name/Id；列表实例组件为 "{listId}|{itemPrimaryKey}|{componentName}"</remarks>
    private static string? GetFormValueKey(ComponentSchema component, object? dataContext)
    {
        var name = component.Name ?? component.Id;
        if (string.IsNullOrEmpty(name))
            return null;

        if (dataContext is ListItemContext ctx)
        {
            var pk = ListDataOperationManager.GetItemPrimaryKey(ctx.Item)?.ToString();
            if (string.IsNullOrEmpty(pk))
                pk = ctx.Index.ToString();
            return $"{ctx.ListId}|{pk}|{name}";
        }

        return name;
    }

    #endregion

    #region 栅格宽度

    /// <summary>
    /// 计算组件宽度百分比（栅格 24 列）
    /// </summary>
    /// <remarks>ItemWidth 未配置(<=4)时按页面布局均分：playout=N 表示每行 N 个字段，即每项占 24/N 列</remarks>
    protected double GetComponentWidthPercent(ComponentSchema component, bool isInRootContainer)
    {
        double columns;
        if (component.Style.ItemWidth > 4)
        {
            columns = component.Style.ItemWidth;
        }
        else if (isInRootContainer)
        {
            var pageLayout = Math.Max(1, PageCascading?.PageLayout ?? 1);
            columns = Math.Max(1, 24d / pageLayout);
        }
        else
        {
            columns = 24;
        }

        return columns / 24d * 100;
    }

    #endregion

    #region 组件渲染

    protected virtual RenderFragment RenderComponent(ComponentSchema component)
        => builder =>
    {
        if (component == null || component.Fragment == null)
            throw new NullReferenceException($"{nameof(component)} or {nameof(component.Fragment)} is null");

        ComponentRegistry?.RegisterRoot(component);

        int index = 0;
        RenderComponentRecursive(component.Id, component.IsSupportDataSource,
            component, component.DataSource, component.Fragment, builder, index);
    };

    private void RenderComponentRecursive(
        string componentId, bool isSupportDataSource,
        ComponentSchema component,
        ComponentDataSourceSchema dataSource,
        ComponentFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index,
        object? dataContext = null,
        InnerContainerCursor? sharedCursor = null)
    {
        ArgumentNullException.ThrowIfNull(componentFragment);

        //TypeName 为空时，使用 DefaultTypeName（dt）
        if (string.IsNullOrEmpty(componentFragment.TypeName))
            componentFragment.TypeName = componentFragment.DefaultTypeName;

        if (string.IsNullOrEmpty(componentFragment.TypeName))
            throw new NullReferenceException($"componentId={componentId}, {nameof(componentFragment.TypeName)}");

        // 显示条件求值：条件为假时跳过渲染（组件显隐联动）
        if (component.VisibleCondition != null
            && !EvaluateVisibleCondition(component.VisibleCondition, dataContext))
        {
            return;
        }

        // 检查是否为条件渲染组件
        if (IsConditionalComponent(componentFragment.TypeName) && component.Cases != null)
        {
            RenderConditionalComponent(componentId, component, dataSource, builder, index, dataContext);
            return;
        }

        // 原生 html 元素（frag.t = "html:{tag}"）：直接渲染原生标签
        if (NativeHtmlElement.IsNativeHtml(componentFragment.TypeName))
        {
            ComponentRegistry?.Register(component);

            var nativeFormValueKey = GetFormValueKey(component, dataContext);
            var cursor = sharedCursor ?? new InnerContainerCursor(component);

            // 选项数据源（radio/checkbox 选项组等）：选项模板为原生 html 时，
            // 将各选项内联渲染到原生容器元素内（原生元素不走组件属性/ChildContent 通道）
            RenderFragment? optionChildren = null;
            if (component.IsSupportDataSource
                && dataSource != null
                && dataSource.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.Option
                && dataSource.DataSourceFragment != null
                && NativeHtmlElement.IsNativeHtml(dataSource.DataSourceFragment.TypeName))
            {
                var options = GetOptionItems(dataSource, dataContext);
                if (options.Count > 0)
                {
                    optionChildren = childBuilder =>
                    {
                        foreach (var opt in options)
                        {
                            RenderNativeHtmlFragment(componentId, null, dataSource.DataSourceFragment,
                                childBuilder, string.Empty, dataSource.DataSourceFragment,
                                dataContext, nativeFormValueKey, null, opt);
                        }
                    };
                }
            }

            RenderNativeHtmlFragment(componentId, component, componentFragment, builder,
                string.Empty, componentFragment, dataContext, nativeFormValueKey, cursor, null,
                optionChildren);
            return;
        }

        Type componentType = componentFragment.TypeName.ResolveType();
        if (componentType == null)
            return; // 无法解析的类型跳过渲染，避免整页崩溃

        ComponentRegistry?.Register(component);

        builder.OpenComponent(index++, componentType);

        // 表单值状态 key（用于 Value 双向绑定）
        var formValueKey = GetFormValueKey(component, dataContext);

        //渲染属性
        RenderComponentAttributes(builder, index, componentId, componentType,
            componentFragment.Attributes, dataContext, formValueKey);

        //渲染组件事件
        RenderComponentEvents(builder, ref index, component, componentType, dataContext);

        if (isSupportDataSource)
        {
            //渲染数据源
            RenderDataSource(componentId, component, dataSource, builder, index, dataContext, formValueKey);
        }
        else if (componentFragment.HasChildren)
        {
            //渲染 ChildContent
            RenderChildFragments(componentId, component, componentFragment, builder, index);
        }
        else if (component.Childrens != null && component.Childrens.Length > 0)
        {
            //渲染组件级子组件（用于列表项模板等场景，子组件保留完整组件语义）
            builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
            {
                int childIndex = 0;
                foreach (var child in component.Childrens)
                {
                    if (child?.Fragment == null)
                        continue;

                    RenderComponentRecursive(child.Id, child.IsSupportDataSource,
                        child, child.DataSource, child.Fragment, childBuilder, childIndex, dataContext);
                }
            }));
        }

        builder.CloseComponent();
    }

    /// <summary>
    /// 判断组件显示条件是否满足（无显示条件时始终显示）
    /// </summary>
    protected bool IsComponentVisible(ComponentSchema component)
    {
        if (component?.VisibleCondition == null)
            return true;

        return EvaluateVisibleCondition(component.VisibleCondition, null);
    }

    /// <summary>
    /// 求值显示条件
    /// </summary>
    private bool EvaluateVisibleCondition(VisibleConditionSchema condition, object? dataContext)
    {
        var context = CreateExpressionContext(dataContext);
        var value = LowCodeExpressionResolver.Resolve(condition.ValueExpr, context);

        switch (condition.Op)
        {
            case VisibleConditionOpEnum.NotEmpty:
                return !string.IsNullOrEmpty(LowCodeExpressionResolver.FormatValue(value));

            case VisibleConditionOpEnum.IsEmpty:
                return string.IsNullOrEmpty(LowCodeExpressionResolver.FormatValue(value));

            case VisibleConditionOpEnum.In:
                {
                    var expect = LowCodeExpressionResolver.ResolveAsString(condition.ExpectExpr, context) ?? string.Empty;
                    var valueStr = LowCodeExpressionResolver.FormatValue(value);
                    return expect.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Any(t => string.Equals(t, valueStr, StringComparison.OrdinalIgnoreCase));
                }

            case VisibleConditionOpEnum.Contains:
                {
                    var expect = LowCodeExpressionResolver.ResolveAsString(condition.ExpectExpr, context) ?? string.Empty;
                    var valueStr = LowCodeExpressionResolver.FormatValue(value);
                    return !string.IsNullOrEmpty(expect) && valueStr.Contains(expect);
                }

            case VisibleConditionOpEnum.NotEquals:
                {
                    var expect = LowCodeExpressionResolver.ResolveAsString(condition.ExpectExpr, context) ?? string.Empty;
                    return !string.Equals(LowCodeExpressionResolver.FormatValue(value), expect, StringComparison.OrdinalIgnoreCase);
                }

            case VisibleConditionOpEnum.Equals:
            default:
                {
                    var expect = LowCodeExpressionResolver.ResolveAsString(condition.ExpectExpr, context) ?? string.Empty;
                    return string.Equals(LowCodeExpressionResolver.FormatValue(value), expect, StringComparison.OrdinalIgnoreCase);
                }
        }
    }

    /// <summary>
    /// 检查是否为条件渲染组件
    /// </summary>
    private bool IsConditionalComponent(string typeName)
    {
        return typeName?.Contains("Conditional") == true;
    }

    /// <summary>
    /// 渲染条件组件
    /// </summary>
    private void RenderConditionalComponent(
        string componentId,
        ComponentSchema component,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index,
        object? dataContext = null)
    {
        if (component.Cases == null || component.Cases.Count == 0)
            return;

        // 从属性中获取条件值
        var conditionValue = GetConditionValue(component, dataContext);
        var conditionKey = conditionValue?.ToString() ?? "";

        // 查找匹配的分支
        ComponentSchema matchedCase = null;
        if (component.Cases.TryGetValue(conditionKey, out matchedCase))
        {
            // 找到匹配的分支
        }
        else if (component.DefaultCase != null)
        {
            // 使用默认分支
            matchedCase = component.DefaultCase;
        }

        // 渲染匹配的分支
        if (matchedCase != null && matchedCase.Fragment != null)
        {
            RenderComponentRecursive(
                matchedCase.Id ?? componentId,
                matchedCase.IsSupportDataSource,
                matchedCase,
                matchedCase.DataSource,
                matchedCase.Fragment,
                builder, index,
                dataContext);
        }
    }

    /// <summary>
    /// 获取条件值
    /// </summary>
    private object? GetConditionValue(ComponentSchema component, object? dataContext)
    {
        if (component.Fragment?.Attributes == null)
            return null;

        // 查找 ConditionValue 属性
        var conditionAttr = component.Fragment.Attributes
            .FirstOrDefault(a => a.AttributeName == "ConditionValue");

        if (conditionAttr == null || conditionAttr.AttributeValue == null)
            return null;

        // 支持数据绑定表达式
        return ResolveAttributeValue(conditionAttr, dataContext);
    }

    #endregion

    #region 原生 html 元素渲染

    /// <summary>
    /// 内部容器游标：按声明顺序消费组件的内部容器（IsInnerContainer），
    /// 将拖入的子组件内联渲染到带 $(DraggableContainer) 标记的原生元素中
    /// </summary>
    private sealed class InnerContainerCursor
    {
        private readonly Queue<ComponentSchema> _containers;

        public InnerContainerCursor(ComponentSchema? component)
        {
            _containers = new Queue<ComponentSchema>(
                component?.Childrens?.Where(c => c is { IsInnerContainer: true })
                ?? Enumerable.Empty<ComponentSchema>());
        }

        public ComponentSchema? TakeNext()
            => _containers.Count > 0 ? _containers.Dequeue() : null;
    }

    /// <summary>
    /// 渲染原生 html 元素 Fragment（frag.t = "html:{tag}"）。
    /// 属性约定：attrn 为 html 属性名（小写）；"class" 多条自动合并；"content" 表示文本内容；
    /// 嵌套子元素属性用 "childs.{i}..." 路径定位。
    /// </summary>
    private void RenderNativeHtmlFragment(string componentId,
        ComponentSchema component,
        ComponentFragmentSchema fragment,
        RenderTreeBuilder builder,
        string path, ComponentFragmentSchema rootFragment,
        object? dataContext, string? formValueKey,
        InnerContainerCursor? cursor,
        (string? OptionValue, string? OptionLabel)? option,
        RenderFragment? extraChildren = null)
    {
        var tagName = NativeHtmlElement.GetTagName(fragment.TypeName);
        if (string.IsNullOrEmpty(tagName))
            return;

        // 资源挂载片段（声明了 js/css 资源或初始化函数）：交由资源挂载点加载（编辑器等纯物料组件）
        if (option == null && HasResourceMount(fragment))
        {
            RenderNativeResourceMount(componentId, fragment, builder, dataContext);
            return;
        }

        builder.OpenElement(0, tagName);

        var state = RenderNativeHtmlAttributes(fragment, rootFragment, path, builder, dataContext, option);

        // 组件级事件（仅根元素）：事件名映射为元素事件（OnClick → onclick）
        if (option == null && path.Length == 0)
            RenderNativeHtmlComponentEvents(component, builder, dataContext);

        // Fragment 级事件（列表模板内按钮等）
        if (option == null)
            RenderNativeHtmlFragmentEvents(fragment, builder, dataContext);

        // 表单值双向绑定（根元素为表单控件时）
        if (option == null && path.Length == 0 && !string.IsNullOrEmpty(formValueKey))
            RenderNativeHtmlValueBinding(builder, tagName, state.InputType, formValueKey, state.StaticValue, state.IsChecked);

        // 选项 input（radio/checkbox 组的选项）：按组值计算选中状态并绑定变更
        if (option != null && tagName == "input"
            && (state.InputType == "radio" || state.InputType == "checkbox")
            && !string.IsNullOrEmpty(formValueKey))
        {
            RenderNativeHtmlOptionInputBinding(builder, state.InputType, state.OptionValue, formValueKey);
        }

        // 内容：$(DraggableContainer) 标记 → 内联渲染内部容器中的子组件；否则渲染文本内容
        var content = state.ContentOverride ?? fragment.Content;
        if (string.Equals(content, NativeHtmlElement.DraggableContainerToken, StringComparison.OrdinalIgnoreCase))
        {
            var inner = cursor?.TakeNext();
            if (inner?.Childrens != null)
            {
                foreach (var child in inner.Childrens)
                {
                    RenderInlineComponent(componentId, child, builder, dataContext);
                }
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
        if (fragment.HasChildren)
        {
            for (var i = 0; i < fragment.ChildFragments.Length; i++)
            {
                RenderNativeHtmlChildFragment(componentId, component, fragment.ChildFragments[i],
                    builder, NativeHtmlElement.ChildPath(path, i), rootFragment,
                    dataContext, formValueKey, cursor, option);
            }
        }

        // 附加子内容（如选项数据源生成的选项元素）
        if (extraChildren != null)
        {
            extraChildren(builder);
        }

        builder.CloseElement();
    }

    /// <summary>
    /// 判断片段是否声明了资源挂载（js/css 资源清单或初始化函数）
    /// </summary>
    private static bool HasResourceMount(ComponentFragmentSchema fragment)
        => (fragment.Resources != null && fragment.Resources.Length > 0)
           || !string.IsNullOrWhiteSpace(fragment.InitFunction);

    /// <summary>
    /// 渲染资源挂载点：提取片段的样式属性与组件选项，交由 NativeResourceMount 加载资源并初始化
    /// </summary>
    private void RenderNativeResourceMount(string componentId,
        ComponentFragmentSchema fragment,
        RenderTreeBuilder builder, object? dataContext)
    {
        string? classValue = null;
        string? styleValue = null;
        var options = new Dictionary<string, object?>();

        foreach (var attr in fragment.Attributes)
        {
            if (string.IsNullOrEmpty(attr?.AttributeName))
                continue;
            // 嵌套路径属性不作为挂载选项
            if (attr.AttributeName.Contains('.'))
                continue;

            var resolved = ResolveAttributeValue(attr, dataContext)?.ToString();
            var name = attr.AttributeName.ToLowerInvariant();

            if (name == "class")
                classValue = string.IsNullOrEmpty(classValue) ? resolved : $"{classValue} {resolved}";
            else if (name == "style")
                styleValue = resolved;
            else if (name != "id")
                options[attr.AttributeName] = resolved;
        }

        builder.OpenComponent<NativeResourceMount>(0);
        builder.AddAttribute(1, "MountId", $"res-{componentId}");
        builder.AddAttribute(2, "Class", classValue);
        builder.AddAttribute(3, "Style", styleValue);
        builder.AddAttribute(4, "Resources", fragment.Resources);
        builder.AddAttribute(5, "InitFunction", fragment.InitFunction);
        builder.AddAttribute(6, "Options", options);
        builder.CloseComponent();
    }

    /// <summary>
    /// 渲染子 Fragment：原生 html 走元素渲染，.NET 类型走组件渲染
    /// </summary>
    private void RenderNativeHtmlChildFragment(string componentId,
        ComponentSchema component,
        ComponentFragmentSchema fragment,
        RenderTreeBuilder builder,
        string path, ComponentFragmentSchema rootFragment,
        object? dataContext, string? formValueKey,
        InnerContainerCursor? cursor,
        (string? OptionValue, string? OptionLabel)? option)
    {
        if (NativeHtmlElement.IsNativeHtml(fragment.TypeName))
        {
            RenderNativeHtmlFragment(componentId, component, fragment, builder, path, rootFragment,
                dataContext, formValueKey, cursor, option);
            return;
        }

        RenderComponentRecursive(componentId, false, component, null, fragment, builder, 0, dataContext);
    }

    /// <summary>
    /// 内联渲染组件（内部容器的子组件、容器组件自身不再产生包裹）
    /// </summary>
    private void RenderInlineComponent(string componentId, ComponentSchema component,
        RenderTreeBuilder builder, object? dataContext)
    {
        if (component == null)
            return;

        if (component.IsContainer && component.Fragment == null)
        {
            if (component.Childrens != null)
            {
                foreach (var child in component.Childrens)
                    RenderInlineComponent(componentId, child, builder, dataContext);
            }
            return;
        }

        if (component.Fragment != null)
        {
            RenderComponentRecursive(component.Id, component.IsSupportDataSource,
                component, component.DataSource, component.Fragment, builder, 0, dataContext);
        }
    }

    private sealed class NativeHtmlAttributeState
    {
        public string? InputType { get; set; }
        public string? StaticValue { get; set; }
        public bool IsChecked { get; set; }
        public string? OptionValue { get; set; }
        public string? ContentOverride { get; set; }
    }

    /// <summary>
    /// 渲染原生 html 元素属性（支持表达式解析与选项占位符替换），返回属性状态
    /// </summary>
    private NativeHtmlAttributeState RenderNativeHtmlAttributes(
        ComponentFragmentSchema fragment,
        ComponentFragmentSchema rootFragment,
        string path, RenderTreeBuilder builder,
        object? dataContext, (string? OptionValue, string? OptionLabel)? option)
    {
        var state = new NativeHtmlAttributeState();
        var classes = new List<string>();
        var isRoot = ReferenceEquals(fragment, rootFragment);

        void Apply(ComponentAttributeFragmentSchema attr, string attrName)
        {
            // 表达式解析（$(form.x)、$(item.x)、$query(x) 等）
            var resolved = ResolveAttributeValue(attr, dataContext);
            var value = resolved?.ToString();

            if (option.HasValue)
                value = NativeHtmlElement.SubstituteOptionToken(value, option.Value.OptionValue, option.Value.OptionLabel);

            // 类开关（class:片段 + 布尔）：为 true 时合并 class 片段
            if (NativeHtmlElement.IsClassToggle(attrName))
            {
                if (bool.TryParse(value, out var toggleOn) && toggleOn)
                    classes.Add(NativeHtmlElement.GetClassToggleFragment(attrName));
                return;
            }

            if (string.Equals(attrName, "class", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(value))
                    classes.Add(value);
                return;
            }

            if (string.Equals(attrName, "content", StringComparison.OrdinalIgnoreCase))
            {
                state.ContentOverride = value;
                return;
            }

            if (string.IsNullOrEmpty(attrName))
                return;

            var lowerName = attrName.ToLowerInvariant();

            // 记录 input 类型与静态值/选中态（供表单绑定使用）
            if (lowerName == "type" && string.Equals(GetTagNameOf(fragment), "input", StringComparison.Ordinal))
                state.InputType = value?.ToLowerInvariant();
            if (lowerName == "value")
            {
                state.StaticValue = value;
                state.OptionValue = value;
            }
            if (lowerName == "checked" && bool.TryParse(value, out var checkedValue))
                state.IsChecked = checkedValue;

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

        foreach (var attr in fragment.Attributes)
        {
            if (string.IsNullOrEmpty(attr?.AttributeName))
                continue;

            var (attrPath, attrName) = NativeHtmlElement.ParseAttributePath(attr.AttributeName);

            if (isRoot && attrPath.Length > 0)
                continue;
            if (!isRoot && attrPath.Length > 0 && attrPath != path)
                continue;

            Apply(attr, attrName);
        }

        if (!isRoot)
        {
            foreach (var attr in rootFragment.Attributes)
            {
                if (string.IsNullOrEmpty(attr?.AttributeName))
                    continue;

                var (attrPath, attrName) = NativeHtmlElement.ParseAttributePath(attr.AttributeName);
                if (attrPath != path)
                    continue;

                Apply(attr, attrName);
            }
        }

        if (classes.Count > 0)
            builder.AddAttribute(0, "class", string.Join(" ", classes));

        return state;
    }

    private static string? GetTagNameOf(ComponentFragmentSchema fragment)
        => NativeHtmlElement.GetTagName(fragment.TypeName);

    /// <summary>
    /// 组件级事件绑定到根元素（事件名转小写：OnClick → onclick）
    /// </summary>
    private void RenderNativeHtmlComponentEvents(ComponentSchema component,
        RenderTreeBuilder builder, object? dataContext)
    {
        if (component.Events == null || component.Events.Count == 0)
            return;

        var eventGroups = component.Events
            .Where(e => !string.IsNullOrEmpty(e.EventName))
            .GroupBy(e => e.EventName);

        foreach (var eventGroup in eventGroups)
        {
            var elementEventName = NativeHtmlElement.ToElementEventName(eventGroup.Key);
            if (string.IsNullOrEmpty(elementEventName))
                continue;

            var events = eventGroup.ToList();
            builder.AddAttribute(0, elementEventName,
                EventCallback.Factory.Create(this, () => HandleEventChainAsync(component, events, dataContext)));
        }
    }

    /// <summary>
    /// Fragment 级事件绑定（列表模板内的按钮等，仅 Data 类动作）
    /// </summary>
    private void RenderNativeHtmlFragmentEvents(ComponentFragmentSchema fragment,
        RenderTreeBuilder builder, object? dataContext)
    {
        if (fragment.Events == null || fragment.Events.Count == 0)
            return;

        foreach (var ev in fragment.Events)
        {
            if (ev.EventHandlerType != EventTargetTypeEnum.Data)
                continue;
            if (ev.EventName != "OnClick")
                continue;

            var listId = CurrentListId;
            var itemIndex = 0;
            if (dataContext is ListItemContext ctx)
            {
                listId = ctx.ListId;
                itemIndex = ctx.Index;
            }

            builder.AddAttribute(0, "onclick",
                CreateButtonClickHandler(ev.EventDataActionType, listId, itemIndex));
        }
    }

    /// <summary>
    /// 根元素表单控件值双向绑定：value/checked 来自 FormState（用户已输入值优先），
    /// oninput/onchange 回写 FormState（驱动显示条件联动与表单提交收集）
    /// </summary>
    private void RenderNativeHtmlValueBinding(RenderTreeBuilder builder,
        string tagName, string? inputType, string formValueKey,
        string? staticValue, bool staticChecked)
    {
        if (FormState == null)
            return;

        // input[checkbox]/input[radio]：布尔/单值选中绑定
        if (tagName == "input" && (inputType == "checkbox" || inputType == "radio"))
        {
            var current = FormState.HasValue(formValueKey)
                ? FormState.GetValue(formValueKey)?.ToString()
                : (staticChecked ? "true" : "false");
            if (!FormState.HasValue(formValueKey))
                FormState.SetValueSilently(formValueKey, current ?? "false");

            var isChecked = string.Equals(current, "true", StringComparison.OrdinalIgnoreCase);
            if (isChecked)
                builder.AddAttribute(0, "checked", true);

            builder.AddAttribute(0, "onchange",
                EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
                {
                    var value = e.Value?.ToString() ?? string.Empty;
                    var newValue = value.Contains("true", StringComparison.OrdinalIgnoreCase) || value == "1"
                        ? "true" : "false";
                    FormState.SetValue(formValueKey, newValue);
                }));
            return;
        }

        if (!NativeHtmlElement.IsFormControl(tagName, inputType))
            return;

        // 文本类控件：value 绑定
        var currentValue = FormState.HasValue(formValueKey)
            ? FormState.GetValue(formValueKey)?.ToString()
            : staticValue;
        if (!FormState.HasValue(formValueKey) && currentValue != null)
            FormState.SetValueSilently(formValueKey, currentValue);

        builder.AddAttribute(0, "value", currentValue ?? string.Empty);

        // 连续输入类用 oninput，提交类（date/time/number 等）用 onchange
        var eventName = inputType is null or "text" or "search" or "password" or "email" or "tel" or "url"
            || tagName == "textarea"
            ? "oninput" : "onchange";

        builder.AddAttribute(0, eventName,
            EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
            {
                FormState.SetValue(formValueKey, e.Value?.ToString() ?? string.Empty);
            }));
    }

    /// <summary>
    /// 选项 input（radio/checkbox 组）绑定：按组当前值计算选中状态，变更时回写组值
    /// </summary>
    private void RenderNativeHtmlOptionInputBinding(RenderTreeBuilder builder,
        string inputType, string? optionValue, string formValueKey)
    {
        if (FormState == null || string.IsNullOrEmpty(optionValue))
            return;

        var groupValue = FormState.HasValue(formValueKey)
            ? FormState.GetValue(formValueKey)?.ToString()
            : null;

        bool isChecked;
        if (inputType == "radio")
            isChecked = string.Equals(groupValue, optionValue, StringComparison.Ordinal);
        else
            isChecked = (groupValue ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Contains(optionValue);

        if (isChecked)
            builder.AddAttribute(0, "checked", true);

        builder.AddAttribute(0, "onchange",
            EventCallback.Factory.Create<ChangeEventArgs>(this, e =>
            {
                if (inputType == "radio")
                {
                    FormState.SetValue(formValueKey, optionValue);
                    return;
                }

                // checkbox 组：值以英文逗号分隔
                var current = FormState.HasValue(formValueKey)
                    ? FormState.GetValue(formValueKey)?.ToString()
                    : string.Empty;
                var selected = current?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList() ?? new List<string>();
                if (selected.Contains(optionValue))
                    selected.Remove(optionValue);
                else
                    selected.Add(optionValue);
                FormState.SetValue(formValueKey, string.Join(",", selected));
            }));
    }

    #endregion

    #region 属性渲染与值绑定

    /// <summary>
    /// 重写属性渲染，支持数据上下文与表单值双向绑定
    /// </summary>
    protected virtual void RenderComponentAttributes(RenderTreeBuilder builder, int index,
        string componentId, Type componentType,
        ComponentAttributeFragmentSchema[] attributes, object dataContext, string? formValueKey)
    {
        if (attributes == null || attributes.Length == 0)
            return;

        foreach (var attr in attributes)
        {
            if (string.IsNullOrEmpty(attr.AttributeName))
                continue;

            var propertyInfo = componentType.GetProperty(attr.AttributeName);
            if (propertyInfo == null)
                continue;

            // 解析数据绑定表达式
            var resolvedValue = ResolveAttributeValue(attr, dataContext);

            // Value 属性：表单状态优先（用户已输入的值），并注册 ValueChanged 回写
            if (attr.AttributeName == "Value" && !string.IsNullOrEmpty(formValueKey))
            {
                if (FormState != null && FormState.HasValue(formValueKey))
                {
                    resolvedValue = FormState.GetValue(formValueKey);
                }
                else if (FormState != null && resolvedValue != null)
                {
                    // 初始值静默播种到表单状态，供显示条件联动与提交收集使用
                    FormState.SetValueSilently(formValueKey, resolvedValue);
                }

                var changedProperty = componentType.GetProperty("ValueChanged");
                if (changedProperty != null && FormState != null)
                {
                    var stateKey = formValueKey;
                    var callback = EventCallbackHelper.CreateWithArg(this, changedProperty.PropertyType,
                        newValue => FormState.SetValue(stateKey, newValue));
                    if (callback != null)
                        builder.AddAttribute(index++, "ValueChanged", callback);
                }
            }

            // 使用属性的实际类型进行转换，而不是使用 attr.AttributeClrType
            var realType = propertyInfo.PropertyType;

            // 转换为属性的实际类型
            object? realValue = resolvedValue;
            if (realValue != null)
            {
                // 先确保值是字符串类型
                string stringValue = realValue.ToString();
                realValue = stringValue.ConvertToRealType(realType);
            }

            builder.AddAttribute(index++, attr.AttributeName, realValue);
        }
    }

    /// <summary>
    /// 解析属性值，支持表达式（$(item.x)、$(form.x)、$query(x) 等）
    /// </summary>
    private object? ResolveAttributeValue(ComponentAttributeFragmentSchema attr, object? dataContext)
    {
        if (attr.AttributeValue == null)
            return null;

        var valueStr = attr.AttributeValue.ToString();
        if (string.IsNullOrEmpty(valueStr))
            return null;

        if (!LowCodeExpressionResolver.ContainsExpression(valueStr))
            return attr.AttributeValue;

        return LowCodeExpressionResolver.Resolve(valueStr, CreateExpressionContext(dataContext));
    }

    #endregion

    #region 组件事件消费

    /// <summary>
    /// 渲染组件事件绑定（同名事件按配置顺序串行执行）
    /// </summary>
    private void RenderComponentEvents(RenderTreeBuilder builder, ref int index,
        ComponentSchema component, Type componentType, object? dataContext)
    {
        if (component.Events == null || component.Events.Count == 0)
            return;

        var eventGroups = component.Events
            .Where(e => !string.IsNullOrEmpty(e.EventName))
            .GroupBy(e => e.EventName);

        foreach (var eventGroup in eventGroups)
        {
            var eventProperty = componentType.GetProperty(eventGroup.Key);
            if (eventProperty == null)
                continue;

            var events = eventGroup.ToList();
            var callback = EventCallbackHelper.CreateWithoutArg(this, eventProperty.PropertyType,
                () => HandleEventChainAsync(component, events, dataContext));
            if (callback != null)
                builder.AddAttribute(index++, eventGroup.Key, callback);
        }
    }

    /// <summary>
    /// 按顺序执行事件链（某个事件返回 true 表示中断后续执行，如校验失败）
    /// </summary>
    protected async Task HandleEventChainAsync(ComponentSchema component, IList<EventSchema> events, object? dataContext)
    {
        foreach (var ev in events)
        {
            var abort = await HandleEventAsync(component, ev, dataContext);
            if (abort)
                break;
        }
    }

    /// <summary>
    /// 执行单个事件
    /// </summary>
    /// <returns>true 表示中断事件链</returns>
    protected virtual async Task<bool> HandleEventAsync(ComponentSchema component, EventSchema ev, object? dataContext)
    {
        switch (ev.EventHandlerType)
        {
            case EventTargetTypeEnum.Page:
                await HandlePageEventAsync(ev, dataContext);
                return false;

            case EventTargetTypeEnum.Data:
                return await HandleDataEventAsync(component, ev, dataContext);

            case EventTargetTypeEnum.Custom:
                if (ev.EventCustomLanguage == EventCustomLanguageEnum.JavaScript
                    && !string.IsNullOrEmpty(ev.EventCustomScript))
                {
                    await JSRuntime.InvokeVoidAsync("eval", ev.EventCustomScript);
                }
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// 页面跳转事件
    /// </summary>
    private async Task HandlePageEventAsync(EventSchema ev, object? dataContext)
    {
        if (string.IsNullOrEmpty(ev.EventTargetId) || PageCascading == null)
            return;

        var url = BuildPageUrl(ev, dataContext);

        int.TryParse(ev.EventTargetAction, out var handlerValue);
        var handler = (EventPageHandlerTypeEnum)handlerValue;

        switch (handler)
        {
            case EventPageHandlerTypeEnum.Blank:
                await JSRuntime.InvokeVoidAsync("open", url, "_blank");
                break;

            case EventPageHandlerTypeEnum.Refresh:
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
                break;

            case EventPageHandlerTypeEnum.Self:
            default:
                NavigationManager.NavigateTo(url);
                break;
        }
    }

    /// <summary>
    /// 构建页面跳转 URL（含固定参数与行数据参数映射，按当前路由判断设计态/运行态前缀）
    /// </summary>
    private string BuildPageUrl(EventSchema ev, object? dataContext)
    {
        var currentPath = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).AbsolutePath;
        var prefix = currentPath.StartsWith("/designer", StringComparison.OrdinalIgnoreCase)
            ? "/designer"
            : "/app";

        var url = $"{prefix}/{PageCascading.AppId}/{ev.EventTargetId}";
        var context = CreateExpressionContext(dataContext);
        var item = context.Item;

        var queryParts = new List<string>();
        if (ev.EventArgs != null)
        {
            foreach (var kv in ev.EventArgs)
            {
                var value = LowCodeExpressionResolver.ResolveAsString(kv.Value, context);
                if (!string.IsNullOrEmpty(value))
                    queryParts.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(value)}");
            }
        }

        if (ev.RowDataParams != null && item != null)
        {
            foreach (var kv in ev.RowDataParams)
            {
                var value = LowCodeExpressionResolver.GetMemberValue(item, kv.Value)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    queryParts.Add($"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(value)}");
            }
        }

        return queryParts.Count > 0
            ? $"{url}?{string.Join("&", queryParts)}"
            : url;
    }

    /// <summary>
    /// 数据操作事件
    /// </summary>
    /// <returns>true 表示中断事件链</returns>
    private async Task<bool> HandleDataEventAsync(ComponentSchema component, EventSchema ev, object? dataContext)
    {
        var listId = ResolveListId(ev, dataContext);
        var itemIndex = dataContext is ListItemContext ctx ? ctx.Index : 0;

        switch (ev.EventDataActionType)
        {
            case EventDataActionTypeEnum.MoveUp:
            case EventDataActionTypeEnum.MoveDown:
            case EventDataActionTypeEnum.DeleteRow:
            case EventDataActionTypeEnum.CopyRow:
            case EventDataActionTypeEnum.AddRow:
                HandleListDataAction(ev.EventDataActionType, listId, itemIndex);
                return false;

            case EventDataActionTypeEnum.RefreshData:
                if (!string.IsNullOrEmpty(listId))
                    await ReloadListAsync(listId);
                return false;

            case EventDataActionTypeEnum.SaveRow:
            case EventDataActionTypeEnum.SaveList:
                if (string.IsNullOrEmpty(listId))
                    return true;
                return !await SaveListAsync(listId);

            case EventDataActionTypeEnum.SaveForm:
                return !await SaveFormAsync(ev);

            case EventDataActionTypeEnum.UpdateRow:
                await UpdateRowAsync(ev, dataContext);
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// 解析事件关联的列表组件 Id
    /// </summary>
    private string ResolveListId(EventSchema ev, object? dataContext)
    {
        if (dataContext is ListItemContext ctx && !string.IsNullOrEmpty(ctx.ListId))
            return ctx.ListId;

        if (ev.EventArgs != null
            && ev.EventArgs.TryGetValue("listid", out var configuredListId)
            && !string.IsNullOrEmpty(configuredListId))
            return configuredListId;

        return CurrentListId;
    }

    #endregion

    #region 表单保存与校验

    /// <summary>
    /// 保存表单：收集表单状态写入页面数据源
    /// </summary>
    /// <returns>是否保存成功</returns>
    protected async Task<bool> SaveFormAsync(EventSchema ev)
    {
        var tableName = PageCascading?.DataSourceName;
        if (string.IsNullOrEmpty(tableName))
        {
            Toast.Error("页面未配置数据源，无法保存");
            return false;
        }

        // 收集表单状态（普通组件值，排除列表实例值与内部 key）
        var fields = new List<FormFieldDto>();
        foreach (var kv in FormState.GetAllValues())
        {
            if (kv.Key.Contains('|') || kv.Key.StartsWith("__"))
                continue;

            fields.Add(new FormFieldDto
            {
                Name = kv.Key,
                TypeName = typeof(string).FullName,
                Value = kv.Value
            });
        }

        // 编辑已有记录时确保主键存在
        const string pkName = ListDataOperationManager.PrimaryKeyFieldName;
        var id = FormState.GetValue(pkName)?.ToString();
        if (string.IsNullOrEmpty(id))
            id = GetQueryValue("id");
        if (!string.IsNullOrEmpty(id) && !fields.Any(f => f.Name == pkName))
        {
            fields.Add(new FormFieldDto
            {
                Name = pkName,
                TypeName = typeof(string).FullName,
                Value = id
            });
        }

        // 提交前校验
        var validationErrors = await ValidateFormStateAsync();
        if (validationErrors.Count > 0)
        {
            Toast.Error($"表单校验失败：\n{string.Join("\n", validationErrors)}");
            return false;
        }

        try
        {
            var dto = new FormDataDto
            {
                Name = tableName,
                Fields = fields
            };
            var savedId = (await FormDataAppService.SaveAsync(dto)).Data;
            FormState.SetValueSilently(PageFormStateService.LastIdKey, savedId);
            Toast.Success("保存成功");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "表单保存失败");
            Toast.Error($"保存时发生错误：{ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 校验表单状态（按页面组件树的校验规则，跳过显示条件不满足的组件）
    /// </summary>
    private async Task<IList<string>> ValidateFormStateAsync()
    {
        var errors = new List<string>();
        var roots = ComponentRegistry?.GetRoots();
        if (roots == null)
            return errors;

        foreach (var root in roots)
        {
            await ValidateComponentRecursiveAsync(root, null, errors);
        }

        return errors;
    }

    private async Task ValidateComponentRecursiveAsync(ComponentSchema component, object? dataContext, List<string> errors)
    {
        // 显示条件不满足的组件不参与校验
        if (component.VisibleCondition != null
            && !EvaluateVisibleCondition(component.VisibleCondition, dataContext))
            return;

        // 校验规则执行
        if (component.ValidationRules != null && component.ValidationRules.Count > 0)
        {
            var stateKey = GetFormValueKey(component, dataContext);
            if (!string.IsNullOrEmpty(stateKey))
            {
                var value = FormState.GetValue(stateKey);
                var result = FieldValidator.Validate(value, component.ValidationRules);
                if (!result.IsValid)
                {
                    errors.Add($"{component.Label ?? component.Name ?? component.Id}: {result.ErrorMessage}");
                }
            }
        }

        // 列表组件：逐行校验实例
        var isList = component.DataSource?.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.List
            && component.DataSource?.ItemTemplate != null;
        if (isList)
        {
            var rows = ListDataManager.GetListData(component.Id);
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var instanceContext = new ListItemContext { Item = row, Index = i, ListId = component.Id };
                await ValidateComponentRecursiveAsync(component.DataSource.ItemTemplate, instanceContext, errors);
            }
            return;
        }

        // 递归子组件
        if (component.Childrens != null)
        {
            foreach (var child in component.Childrens)
            {
                await ValidateComponentRecursiveAsync(child, dataContext, errors);
            }
        }
    }

    #endregion

    #region List 数据保存与行更新

    /// <summary>
    /// 保存列表数据到数据库（按保存映射构造行数据）
    /// </summary>
    /// <returns>是否保存成功</returns>
    protected async Task<bool> SaveListAsync(string listId)
    {
        var listComponent = ComponentRegistry?.GetById(listId);
        var listDs = listComponent?.DataSource?.ListDataSource;

        var saveToDsId = listDs?.SaveToDataSourceId ?? listDs?.TableDataSourceId;
        if (string.IsNullOrEmpty(saveToDsId))
        {
            Toast.Warning("列表未配置保存目标数据源");
            return false;
        }

        var forceInsert = listDs?.SaveMode == ListSaveModeEnum.InsertNew;

        var rows = ListDataManager.GetListData(listId);

        // 列表从未加载过（如显示条件不满足的隐藏列表）时静默跳过
        if (rows.Count == 0 && !ListDataManager.HasListData(listId))
            return true;

        try
        {
            // 同步删除编辑过程中被移除的行（仅 Upsert 模式且数据来源与保存目标一致时）
            if (!forceInsert
                && ListDataManager.IsDbSynced(listId)
                && listDs?.TableDataSourceId == saveToDsId)
            {
                foreach (var deletedId in ListDataManager.GetAndClearDeletedIds(listId))
                {
                    await TableDataAppService.DeleteAsync(new TableDataDeleteInput
                    {
                        AppId = PageCascading.AppId,
                        PageId = PageCascading.PageId,
                        DataSourceId = saveToDsId,
                        Id = deletedId
                    });
                }
            }

            var rowsForSave = ListDataManager.GetListData(listId);
            MergeFormStateIntoRows(listId, rowsForSave);

            foreach (var row in rowsForSave)
            {
                var rowData = ToRowDictionary(row);

                // 应用保存字段映射（在行数据基础上叠加/覆盖）
                if (listDs?.SaveMap != null && listDs.SaveMap.Count > 0)
                {
                    var rowContext = CreateExpressionContext(row);
                    foreach (var kv in listDs.SaveMap)
                    {
                        var value = LowCodeExpressionResolver.Resolve(kv.Value, rowContext);
                        if (value != null)
                            rowData[kv.Key] = value;
                    }
                }

                await TableDataAppService.SaveAsync(new TableDataSaveInput
                {
                    AppId = PageCascading.AppId,
                    DataSourceId = saveToDsId,
                    RowData = rowData,
                    ForceInsert = forceInsert
                });
            }

            // 保存后刷新列表（加载来源与保存目标一致时）
            if (!string.IsNullOrEmpty(listDs?.TableDataSourceId) && listDs.TableDataSourceId == saveToDsId)
                await ReloadListAsync(listId);

            Toast.Success("保存成功");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "列表数据保存失败: listId={ListId}", listId);
            Toast.Error($"保存时发生错误：{ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 更新当前行字段（按事件参数）
    /// </summary>
    private async Task UpdateRowAsync(EventSchema ev, object? dataContext)
    {
        if (dataContext is not ListItemContext ctx || PageCascading == null)
            return;

        var listComponent = ComponentRegistry?.GetById(ctx.ListId);
        var dsId = listComponent?.DataSource?.ListDataSource?.TableDataSourceId
            ?? listComponent?.DataSource?.DataSourceId;

        var pk = ListDataOperationManager.GetItemPrimaryKey(ctx.Item)?.ToString();
        if (string.IsNullOrEmpty(dsId) || string.IsNullOrEmpty(pk))
            return;

        var updateData = new Dictionary<string, object>();
        var context = CreateExpressionContext(dataContext);
        if (ev.EventArgs != null)
        {
            foreach (var kv in ev.EventArgs)
            {
                var value = LowCodeExpressionResolver.Resolve(kv.Value, context);
                if (value != null)
                    updateData[kv.Key] = value;
            }
        }

        if (updateData.Count == 0)
            return;

        try
        {
            await TableDataAppService.UpdateAsync(new TableDataUpdateInput
            {
                AppId = PageCascading.AppId,
                PageId = PageCascading.PageId,
                DataSourceId = dsId,
                Id = pk,
                UpdateData = updateData
            });

            await ReloadListAsync(ctx.ListId);
            Toast.Success("操作成功");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "更新行数据失败");
            Toast.Error($"操作失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 行数据转字典
    /// </summary>
    private static Dictionary<string, object> ToRowDictionary(object row)
    {
        if (row is Dictionary<string, object> dict)
            return new Dictionary<string, object>(dict);

        var result = new Dictionary<string, object>();
        if (row == null)
            return result;

        foreach (var prop in row.GetType().GetProperties())
        {
            var value = prop.GetValue(row);
            if (value != null)
                result[prop.Name] = value;
        }
        return result;
    }

    /// <summary>
    /// 将表单状态中列表实例的输入值回写到行数据（组件 Name 即行字段名）
    /// </summary>
    private void MergeFormStateIntoRows(string listId, IList<object> rows)
    {
        if (FormState == null || rows == null)
            return;

        foreach (var row in rows)
        {
            if (row is not Dictionary<string, object> dict)
                continue;

            var pk = ListDataOperationManager.GetItemPrimaryKey(row)?.ToString();
            if (string.IsNullOrEmpty(pk))
                continue;

            var prefix = $"{listId}|{pk}|";
            foreach (var kv in FormState.GetAllValues())
            {
                if (!kv.Key.StartsWith(prefix))
                    continue;

                var fieldName = kv.Key.Substring(prefix.Length);
                if (!string.IsNullOrEmpty(fieldName))
                    dict[fieldName] = kv.Value;
            }
        }
    }

    #endregion

    #region 数据源渲染

    private void RenderDataSource(string componentId,
        ComponentSchema component,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index,
        object? dataContext, string? formValueKey)
    {
        if (dataSource == null)
            return;

        if (dataSource.DataSourceGroupType == ComponentDataSourceGroupTypeEnum.Option)
        {
            switch (dataSource.DataSourceType)
            {
                case ComponentDataSourceTypeEnum.Fiexd:
                    RenderOptionDataSource(componentId, dataSource, builder, index, dataContext, formValueKey);
                    break;
                case ComponentDataSourceTypeEnum.Expression:
                    RenderDynamicOptionDataSource(componentId, dataSource, builder, index, dataContext, formValueKey);
                    break;
                case ComponentDataSourceTypeEnum.SQL:
                    break;
                case ComponentDataSourceTypeEnum.API:
                    break;
                default:
                    // 未显式指定类型时，按已配置的内容渲染
                    if (!string.IsNullOrEmpty(dataSource.DynamicOptionExpr))
                        RenderDynamicOptionDataSource(componentId, dataSource, builder, index, dataContext, formValueKey);
                    else if (dataSource.FiexdOptionDataSource != null && dataSource.FiexdOptionDataSource.Count > 0)
                        RenderOptionDataSource(componentId, dataSource, builder, index, dataContext, formValueKey);
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
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index,
        object? dataContext, string? formValueKey)
    {
        var options = GetOptionItems(dataSource, dataContext);
        if (options.Count == 0)
            return;

        RenderOptionItems(componentId, dataSource, builder, index, options, dataContext, formValueKey);
    }

    /// <summary>
    /// 解析选项数据源的选项列表（固定值或表达式）
    /// </summary>
    private List<(string? Value, string? Label)> GetOptionItems(ComponentDataSourceSchema dataSource, object? dataContext)
    {
        if (dataSource.DataSourceType == ComponentDataSourceTypeEnum.Expression
            || (dataSource.DataSourceType == ComponentDataSourceTypeEnum.None
                && !string.IsNullOrEmpty(dataSource.DynamicOptionExpr)))
        {
            var rawValue = LowCodeExpressionResolver.ResolveAsString(
                dataSource.DynamicOptionExpr, CreateExpressionContext(dataContext));
            if (string.IsNullOrWhiteSpace(rawValue))
                return [];

            return rawValue
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line => ((string? Value, string? Label))(line, line))
                .ToList();
        }

        if (dataSource.FiexdOptionDataSource == null)
            return [];

        return dataSource.FiexdOptionDataSource
            .Select(o => (o.Value, o.Label))
            .ToList();
    }

    /// <summary>
    /// 渲染动态选项（表达式解析字段值后按换行拆分为选项）
    /// </summary>
    private void RenderDynamicOptionDataSource(string componentId,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index,
        object? dataContext, string? formValueKey)
    {
        var options = GetOptionItems(dataSource, dataContext);
        if (options.Count == 0)
            return;

        RenderOptionItems(componentId, dataSource, builder, index, options, dataContext, formValueKey);
    }

    /// <summary>
    /// 渲染选项子组件
    /// </summary>
    private void RenderOptionItems(string componentId,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index,
        IList<(string? Value, string? Label)> options,
        object? dataContext, string? formValueKey)
    {
        if (options.Count == 0)
            return;

        // 无 DataSourceFragment（Hc 风格选项组件）时不渲染选项，避免空引用
        if (dataSource.DataSourceFragment == null)
            return;

        // 原生 html 选项 Fragment（如 label > input[radio] + span）：按选项逐个渲染，
        // 替换 $(value)/$(label) 占位符，radio/checkbox 选项绑定组值
        if (NativeHtmlElement.IsNativeHtml(dataSource.DataSourceFragment.TypeName))
        {
            builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
            {
                foreach (var option in options)
                {
                    RenderNativeHtmlFragment(componentId, null, dataSource.DataSourceFragment,
                        childBuilder, string.Empty, dataSource.DataSourceFragment,
                        dataContext, formValueKey, null,
                        (option.Value?.ToString(), option.Label));
                }
            }));
            return;
        }

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            if (string.IsNullOrEmpty(dataSource.DataSourceFragment.TypeName))
                return;

            Type childComponentType = dataSource.DataSourceFragment.TypeName.ResolveType();
            if (childComponentType == null)
                return;

            foreach (var option in options)
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

    private void RenderChildFragments(string componentId,
        ComponentSchema component,
        ComponentFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index)
    {
        if (componentFragment.HasChildren == false)
            return;

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            // 同一组件的多个子元素共享内部容器游标（按声明顺序逐个消费）
            var cursor = new InnerContainerCursor(component);
            foreach (var childFragment in componentFragment.ChildFragments)
            {
                RenderComponentRecursive(componentId, false,
                    component, null, childFragment, childBuilder, index, null, cursor);
            }
        }));
    }

    /// <summary>
    /// 渲染 List 循环数据源
    /// </summary>
    private void RenderListDataSource(string componentId,
        ComponentSchema component,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource.ListDataSource == null)
            return;

        // 获取列表数据（首次访问时异步从数据库加载）
        var listData = GetListDataSource(componentId, dataSource);
        if (listData == null || listData.Count == 0)
            return;

        CurrentListId = componentId;

        // 优先使用 ItemTemplate（支持完整组件配置）
        if (dataSource.ItemTemplate != null)
        {
            RenderListWithItemTemplate(componentId, dataSource.ItemTemplate, listData, builder, index);
        }
        else if (dataSource.DataSourceFragment != null)
        {
            RenderListWithFragment(componentId, dataSource.DataSourceFragment, listData, builder, index);
        }

        // 设置 DataSource 属性
        builder.AddAttribute(index++, "DataSource", listData);
    }

    /// <summary>
    /// 使用 ItemTemplate（完整组件配置）渲染列表项
    /// </summary>
    private void RenderListWithItemTemplate(string componentId,
        ComponentSchema itemTemplate,
        IList<object> listData,
        RenderTreeBuilder builder, int index)
    {
        builder.AddAttribute(index++, "ChildContent", (RenderFragment<object>)((item) => (childBuilder) =>
        {
            if (itemTemplate.Fragment == null)
                return;

            // 获取当前项索引
            var itemIndex = listData.IndexOf(item);

            // 创建包含索引的数据上下文
            var dataContext = new ListItemContext
            {
                Item = item,
                Index = itemIndex,
                ListId = componentId
            };

            // 传递 item 作为数据上下文，支持条件渲染
            RenderComponentRecursive(
                itemTemplate.Id ?? componentId,
                itemTemplate.IsSupportDataSource,
                itemTemplate,
                itemTemplate.DataSource,
                itemTemplate.Fragment,
                childBuilder, index,
                dataContext);
        }));
    }

    /// <summary>
    /// 列表项上下文
    /// </summary>
    public class ListItemContext
    {
        public object Item { get; set; }
        public int Index { get; set; }
        public string ListId { get; set; }
    }

    /// <summary>
    /// 使用 Fragment（简单组件配置）渲染列表项
    /// </summary>
    private void RenderListWithFragment(string componentId,
        ComponentFragmentSchema fragment,
        IList<object> listData,
        RenderTreeBuilder builder, int index)
    {
        // 原生 html 列表项模板：逐行渲染原生元素树（支持 $(item.x) 表达式）
        if (NativeHtmlElement.IsNativeHtml(fragment.TypeName))
        {
            builder.AddAttribute(index++, "ChildContent", (RenderFragment<object>)((item) => (childBuilder) =>
            {
                var itemContext = new ListItemContext
                {
                    Item = item,
                    Index = listData.IndexOf(item),
                    ListId = componentId
                };
                RenderNativeHtmlFragment(componentId, null, fragment, childBuilder,
                    string.Empty, fragment, itemContext, null, null, null);
            }));
            return;
        }

        builder.AddAttribute(index++, "ChildContent", (RenderFragment<object>)((item) => (childBuilder) =>
        {
            if (string.IsNullOrEmpty(fragment.TypeName))
                return;

            Type itemComponentType = fragment.TypeName.ResolveType();
            if (itemComponentType == null)
                return;

            childBuilder.OpenComponent(index++, itemComponentType);

            // 渲染属性，支持数据绑定
            if (fragment.Attributes != null)
            {
                foreach (var attr in fragment.Attributes)
                {
                    if (string.IsNullOrEmpty(attr.AttributeName))
                        continue;

                    var attrValue = ResolveAttributeValue(attr, item);

                    var propertyInfo = itemComponentType.GetProperty(attr.AttributeName);
                    if (propertyInfo != null)
                    {
                        // 使用属性的实际类型进行转换
                        var realType = propertyInfo.PropertyType;

                        // 转换为属性的实际类型
                        object? realValue = attrValue;
                        if (realValue != null)
                        {
                            // 先确保值是字符串类型
                            string stringValue = realValue.ToString();
                            realValue = stringValue.ConvertToRealType(realType);
                        }

                        childBuilder.AddAttribute(index++, attr.AttributeName, realValue);
                    }
                    else
                    {
                        childBuilder.AddAttribute(index++, attr.AttributeName, attrValue);
                    }
                }
            }

            // 渲染子组件
            if (fragment.HasChildren)
            {
                childBuilder.AddAttribute(index++, "ChildContent", (RenderFragment)(grandchildBuilder =>
                {
                    foreach (var childFragment in fragment.ChildFragments)
                    {
                        RenderFragmentRecursive(componentId, childFragment, grandchildBuilder, index, item);
                    }
                }));
            }

            childBuilder.CloseComponent();
        }));
    }

    /// <summary>
    /// 递归渲染 Fragment
    /// </summary>
    private void RenderFragmentRecursive(string componentId,
        ComponentFragmentSchema fragment,
        RenderTreeBuilder builder, int index,
        object dataContext)
    {
        if (fragment == null || string.IsNullOrEmpty(fragment.TypeName))
            return;

        // 原生 html 元素：直接渲染原生标签（含 Fragment 级事件与 $(item.x) 表达式）
        if (NativeHtmlElement.IsNativeHtml(fragment.TypeName))
        {
            RenderNativeHtmlFragment(componentId, null, fragment, builder,
                string.Empty, fragment, dataContext, null, null, null);
            return;
        }

        Type componentType = fragment.TypeName.ResolveType();
        if (componentType == null)
            return;

        builder.OpenComponent(index++, componentType);

        // 渲染属性
        if (fragment.Attributes != null)
        {
            foreach (var attr in fragment.Attributes)
            {
                if (string.IsNullOrEmpty(attr.AttributeName))
                    continue;

                var attrValue = ResolveAttributeValue(attr, dataContext);

                var propertyInfo = componentType.GetProperty(attr.AttributeName);
                if (propertyInfo != null)
                {
                    // 使用属性的实际类型进行转换
                    var realType = propertyInfo.PropertyType;

                    // 转换为属性的实际类型
                    object? realValue = attrValue;
                    if (realValue != null)
                    {
                        // 先确保值是字符串类型
                        string stringValue = realValue.ToString();
                        realValue = stringValue.ConvertToRealType(realType);
                    }

                    builder.AddAttribute(index++, attr.AttributeName, realValue);
                }
                else
                {
                    builder.AddAttribute(index++, attr.AttributeName, attrValue);
                }
            }
        }

        // 渲染事件绑定
        RenderFragmentEvents(fragment, builder, ref index, dataContext);

        // 递归渲染子组件
        if (fragment.HasChildren)
        {
            builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
            {
                foreach (var childFragment in fragment.ChildFragments)
                {
                    RenderFragmentRecursive(componentId, childFragment, childBuilder, index, dataContext);
                }
            }));
        }

        builder.CloseComponent();
    }

    /// <summary>
    /// 渲染 Fragment 事件绑定
    /// </summary>
    private void RenderFragmentEvents(
        ComponentFragmentSchema fragment,
        RenderTreeBuilder builder,
        ref int index,
        object dataContext)
    {
        if (fragment.Events == null || fragment.Events.Count == 0)
            return;

        foreach (var ev in fragment.Events)
        {
            if (ev.EventHandlerType != EventTargetTypeEnum.Data)
                continue;

            // 获取 List 上下文
            var listId = CurrentListId;
            var itemIndex = 0;

            if (dataContext is ListItemContext ctx)
            {
                listId = ctx.ListId;
                itemIndex = ctx.Index;
            }

            // 根据事件名称绑定对应的事件处理器
            if (ev.EventName == "OnClick")
            {
                var callback = CreateButtonClickHandler(ev.EventDataActionType, listId, itemIndex);
                builder.AddAttribute(index++, "OnClick", callback);
            }
        }
    }

    #endregion

    #region List 数据加载

    /// <summary>
    /// 获取 List 数据源数据
    /// </summary>
    private IList<object> GetListDataSource(string componentId, ComponentDataSourceSchema dataSource)
    {
        var listDs = dataSource.ListDataSource;
        if (listDs == null)
            return new List<object>();

        // 优先使用固定数据（设计时预览/静态行）
        if (listDs.FixedData != null && listDs.FixedData.Count > 0)
        {
            if (!ListDataManager.HasListData(componentId))
            {
                ListDataManager.RegisterListData(componentId, listDs.FixedData.Cast<object>().ToList());
            }
            return ListDataManager.GetListData(componentId);
        }

        // 表数据源：异步从数据库加载
        if (!string.IsNullOrEmpty(listDs.TableDataSourceId))
        {
            if (!_loadedListIds.Contains(componentId) && !_loadingListIds.Contains(componentId))
            {
                _loadingListIds.Add(componentId);
                _ = LoadTableListDataAsync(componentId, listDs);
            }
            return ListDataManager.GetListData(componentId);
        }

        // TODO: 实现 API 数据源加载
        if (dataSource.DataSourceType == ComponentDataSourceTypeEnum.API && listDs.APIDataSource != null)
        {
            // 需要在组件初始化时异步加载
            return new List<object>();
        }

        // TODO: 实现 SQL 数据源加载
        if (dataSource.DataSourceType == ComponentDataSourceTypeEnum.SQL && listDs.SQLDataSource != null)
        {
            // 需要在组件初始化时异步加载
            return new List<object>();
        }

        return ListDataManager.GetListData(componentId);
    }

    /// <summary>
    /// 从表数据源异步加载列表数据
    /// </summary>
    private async Task LoadTableListDataAsync(string componentId, ListDataSourceSchema listDs)
    {
        try
        {
            if (PageCascading == null)
                return;

            var filters = new Dictionary<string, object>();
            if (listDs.Filters != null && listDs.Filters.Count > 0)
            {
                var context = CreateExpressionContext(null);
                foreach (var kv in listDs.Filters)
                {
                    // 配置的过滤条件始终生效，表达式无结果时以空值过滤（避免误加载全量数据）
                    var value = LowCodeExpressionResolver.ResolveAsString(kv.Value, context);
                    filters[kv.Key] = value ?? string.Empty;
                }
            }

            string? sorting = null;
            if (!string.IsNullOrEmpty(listDs.OrderBy))
                sorting = listDs.OrderBy + (listDs.OrderDesc ? " desc" : string.Empty);

            var input = new TableDataInput
            {
                AppId = PageCascading.AppId,
                PageId = PageCascading.PageId,
                DataSourceId = listDs.TableDataSourceId,
                SkipCount = 0,
                MaxResultCount = 1000,
                Sorting = sorting,
                Filters = filters
            };

            var result = (await TableDataAppService.GetListAsync(input)).Data;
            var items = result?.Items?.Cast<object>().ToList() ?? new List<object>();

            // 始终以 DB 数据为准，覆盖可能残留的旧会话数据（ListDataManager 为单例，跨页面导航不会重置）
            ListDataManager.RegisterListData(componentId, items, fromDatabase: true);
            _loadedListIds.Add(componentId);

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "加载列表数据失败: componentId={ComponentId}", componentId);
        }
        finally
        {
            _loadingListIds.Remove(componentId);
        }
    }

    /// <summary>
    /// 重新加载列表数据
    /// </summary>
    protected async Task ReloadListAsync(string listId)
    {
        var listComponent = ComponentRegistry?.GetById(listId);
        var listDs = listComponent?.DataSource?.ListDataSource;
        if (listDs == null || string.IsNullOrEmpty(listDs.TableDataSourceId))
        {
            StateHasChanged();
            return;
        }

        _loadedListIds.Remove(listId);
        _loadingListIds.Remove(listId);
        await LoadTableListDataAsync(listId, listDs);
    }

    #endregion

    #region List 内存操作

    /// <summary>
    /// 处理 List 数据操作事件
    /// </summary>
    protected void HandleListDataAction(EventDataActionTypeEnum actionType, string listId, int itemIndex)
    {
        switch (actionType)
        {
            case EventDataActionTypeEnum.MoveUp:
                if (ListDataManager.MoveUp(listId, itemIndex))
                {
                    ListDataManager.UpdateOrderFields(listId);
                    StateHasChanged();
                }
                break;

            case EventDataActionTypeEnum.MoveDown:
                if (ListDataManager.MoveDown(listId, itemIndex))
                {
                    ListDataManager.UpdateOrderFields(listId);
                    StateHasChanged();
                }
                break;

            case EventDataActionTypeEnum.DeleteRow:
                if (ListDataManager.DeleteItem(listId, itemIndex))
                {
                    ListDataManager.UpdateOrderFields(listId);
                    StateHasChanged();
                }
                break;

            case EventDataActionTypeEnum.CopyRow:
                if (ListDataManager.CopyItem(listId, itemIndex))
                {
                    ListDataManager.UpdateOrderFields(listId);
                    StateHasChanged();
                }
                break;

            case EventDataActionTypeEnum.AddRow:
                ListDataManager.AddDefaultItem(listId);
                ListDataManager.UpdateOrderFields(listId);
                StateHasChanged();
                break;

            case EventDataActionTypeEnum.SaveRow:
                _ = SaveListAsync(listId);
                break;

            case EventDataActionTypeEnum.RefreshData:
                _ = ReloadListAsync(listId);
                break;
        }
    }

    /// <summary>
    /// 创建按钮点击事件处理器
    /// </summary>
    protected EventCallback<MouseEventArgs> CreateButtonClickHandler(
        EventDataActionTypeEnum actionType,
        string listId,
        int itemIndex)
    {
        return EventCallback.Factory.Create<MouseEventArgs>(this, () =>
        {
            HandleListDataAction(actionType, listId, itemIndex);
        });
    }

    #endregion
}
