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
        object? dataContext = null)
    {
        ArgumentNullException.ThrowIfNull(componentFragment);

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
            RenderDataSource(componentId, component, dataSource, builder, index, dataContext);
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
        object? dataContext)
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
                case ComponentDataSourceTypeEnum.Expression:
                    RenderDynamicOptionDataSource(componentId, dataSource, builder, index, dataContext);
                    break;
                case ComponentDataSourceTypeEnum.SQL:
                    break;
                case ComponentDataSourceTypeEnum.API:
                    break;
                default:
                    // 未显式指定类型时，按已配置的内容渲染
                    if (!string.IsNullOrEmpty(dataSource.DynamicOptionExpr))
                        RenderDynamicOptionDataSource(componentId, dataSource, builder, index, dataContext);
                    else if (dataSource.FiexdOptionDataSource != null && dataSource.FiexdOptionDataSource.Count > 0)
                        RenderOptionDataSource(componentId, dataSource, builder, index);
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
        RenderTreeBuilder builder, int index)
    {
        if (dataSource.FiexdOptionDataSource == null
            || dataSource.FiexdOptionDataSource.Count == 0)
            return;

        RenderOptionItems(componentId, dataSource, builder, index,
            dataSource.FiexdOptionDataSource.Select(o => (o.Value, o.Label)).ToList());
    }

    /// <summary>
    /// 渲染动态选项（表达式解析字段值后按换行拆分为选项）
    /// </summary>
    private void RenderDynamicOptionDataSource(string componentId,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index,
        object? dataContext)
    {
        if (string.IsNullOrEmpty(dataSource.DynamicOptionExpr))
            return;

        var rawValue = LowCodeExpressionResolver.ResolveAsString(
            dataSource.DynamicOptionExpr, CreateExpressionContext(dataContext));
        if (string.IsNullOrWhiteSpace(rawValue))
            return;

        var options = rawValue
            .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => ((string?)line, (string?)line))
            .ToList();

        RenderOptionItems(componentId, dataSource, builder, index, options);
    }

    /// <summary>
    /// 渲染选项子组件
    /// </summary>
    private void RenderOptionItems(string componentId,
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index,
        IList<(string? Value, string? Label)> options)
    {
        if (options.Count == 0)
            return;

        // 无 DataSourceFragment（Hc 风格选项组件）时不渲染选项，避免空引用
        if (dataSource.DataSourceFragment == null)
            return;

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
            foreach (var childFragment in componentFragment.ChildFragments)
            {
                RenderComponentRecursive(componentId, false,
                    component, null, childFragment, childBuilder, index);
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

            // 若用户在加载期间已添加行，保留内存数据不覆盖
            if (ListDataManager.GetListData(componentId).Count == 0)
            {
                ListDataManager.RegisterListData(componentId, items, fromDatabase: true);
            }
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
