using H.LowCode.ComponentBase;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.RenderEngine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace H.LowCode.RenderEngineBase;

public abstract class RenderEngineDynamicComponentBase : LowCodeDynamicComponentBase
{
    [CascadingParameter(Name = "pageCascading")]
    public PageCascadingModel PageCascading { get; set; }

    [Inject]
    protected ListDataOperationManager ListDataManager { get; set; }

    // 当前 List 组件 ID，用于事件处理
    protected string CurrentListId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected virtual RenderFragment RenderComponent(ComponentSchema component)
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
        ComponentSchema component,
        ComponentDataSourceSchema dataSource,
        ComponentFragmentSchema componentFragment,
        RenderTreeBuilder builder, int index,
        object? dataContext = null)
    {
        ArgumentNullException.ThrowIfNull(componentFragment);

        if (string.IsNullOrEmpty(componentFragment.TypeName))
            throw new NullReferenceException($"componentId={componentId}, {nameof(componentFragment.TypeName)}");
        
        // 检查是否为条件渲染组件
        if (IsConditionalComponent(componentFragment.TypeName) && component.Cases != null)
        {
            RenderConditionalComponent(componentId, component, dataSource, builder, index, dataContext);
            return;
        }

        Type componentType = componentFragment.TypeName.ResolveType();
        if (componentType == null)
            throw new NullReferenceException($"componentId={componentId}, type={componentFragment.TypeName}");

        builder.OpenComponent(index++, componentType);

        //渲染属性
        RenderComponentAttributes(builder, index, componentId, componentType,
            componentFragment.Attributes, dataContext);

        if (isSupportDataSource)
        {
            //渲染数据源
            RenderDataSource(componentId, component, dataSource, builder, index);
        }
        else if (componentFragment.HasChildren)
        {
            //渲染 ChildContent
            RenderChildFragments(componentId, component, componentFragment, builder, index);
        }

        builder.CloseComponent();
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

    /// <summary>
    /// 重写属性渲染，支持数据上下文
    /// </summary>
    protected virtual void RenderComponentAttributes(RenderTreeBuilder builder, int index,
        string componentId, Type componentType,
        ComponentAttributeFragmentSchema[] attributes, object dataContext)
    {
        if (attributes == null || attributes.Length == 0)
            return;

        foreach (var attr in attributes)
        {
            // 解析数据绑定表达式
            var resolvedValue = ResolveAttributeValue(attr, dataContext);
            
            if (string.IsNullOrEmpty(attr.AttributeName))
                continue;

            var propertyInfo = componentType.GetProperty(attr.AttributeName);
            if (propertyInfo == null)
                continue;

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

    private void RenderDataSource(string componentId,
        ComponentSchema component,
        ComponentDataSourceSchema dataSource,
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
        ComponentDataSourceSchema dataSource,
        RenderTreeBuilder builder, int index)
    {
        if (dataSource.FiexdOptionDataSource == null
            || dataSource.FiexdOptionDataSource.Count == 0)
            return;

        builder.AddAttribute(index++, "ChildContent", (RenderFragment)(childBuilder =>
        {
            if (string.IsNullOrEmpty(dataSource.DataSourceFragment.TypeName))
                throw new NullReferenceException($"componentId={componentId}, {nameof(dataSource.DataSourceFragment.TypeName)}");

            Type childComponentType = dataSource.DataSourceFragment.TypeName.ResolveType();
            if (childComponentType == null)
                throw new NullReferenceException($"componentId={componentId}, type={dataSource.DataSourceFragment.TypeName}");

            foreach (var option in dataSource.FiexdOptionDataSource)
            {
                childBuilder.OpenComponent(index++, childComponentType);
                foreach (var fragAttr in dataSource.DataSourceFragment.Attributes)
                {
                    if (string.IsNullOrEmpty(fragAttr.AttributeName))
                        throw new NullReferenceException($"componentId={componentId}, {nameof(fragAttr.AttributeName)} is null");

                    childBuilder.AddAttribute(index++, fragAttr.AttributeName, option.Value);
                }

                //childBuilder.AddContent(index++, option.Label);
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

        // 获取数据源数据
        var listData = GetListDataSource(dataSource);
        if (listData == null || listData.Count == 0)
            return;

        // 注册列表数据到管理器
        CurrentListId = componentId;
        ListDataManager.RegisterListData(componentId, listData);

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
    protected class ListItemContext
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
                throw new NullReferenceException($"componentId={componentId}, ItemTemplate TypeName is null");

            Type itemComponentType = fragment.TypeName.ResolveType();
            if (itemComponentType == null)
                throw new NullReferenceException($"componentId={componentId}, ItemTemplate type={fragment.TypeName}");

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

    /// <summary>
    /// 获取 List 数据源数据
    /// </summary>
    private IList<object> GetListDataSource(ComponentDataSourceSchema dataSource)
    {
        var listDs = dataSource.ListDataSource;
        if (listDs == null)
            return new List<object>();

        // 优先使用固定数据（设计时）
        if (listDs.FixedData != null && listDs.FixedData.Count > 0)
        {
            return listDs.FixedData.Cast<object>().ToList();
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

        return new List<object>();
    }

    /// <summary>
    /// 解析属性值，支持数据绑定表达式 $(item.fieldName)
    /// </summary>
    private object? ResolveAttributeValue(ComponentAttributeFragmentSchema attr, object? dataContext)
    {
        if (attr.AttributeValue == null)
            return null;

        var valueStr = attr.AttributeValue.ToString();
        if (string.IsNullOrEmpty(valueStr))
            return null;

        // 支持绑定表达式 $(item.fieldName)
        if (valueStr.StartsWith("$(item.") && valueStr.EndsWith(")"))
        {
            var fieldName = valueStr.Substring(7, valueStr.Length - 8); // 提取字段名
            
            // 处理 ListItemContext
            object? dataItem = dataContext;
            if (dataContext is ListItemContext ctx)
            {
                dataItem = ctx.Item;
            }

            if (dataItem is Dictionary<string, object> dict)
            {
                return dict.TryGetValue(fieldName, out object? value) ? value : null;
            }
            else if (dataItem != null)
            {
                // 使用反射获取属性值
                var propInfo = dataItem.GetType().GetProperty(fieldName);
                return propInfo?.GetValue(dataItem);
            }
        }

        return attr.AttributeValue;
    }

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
                // 保存操作通过页面事件配置调用数据源 API
                // 此处只记录日志，实际保存由页面事件触发
                OnListDataSave?.Invoke(listId, ListDataManager.GetListData(listId));
                Console.WriteLine($"[SaveRow] List {listId} save triggered");
                break;

            case EventDataActionTypeEnum.RefreshData:
                // 刷新数据通过页面事件配置调用数据源 API
                OnListDataRefresh?.Invoke(listId);
                Console.WriteLine($"[RefreshData] List {listId} refresh triggered");
                break;
        }
    }

    /// <summary>
    /// 列表数据保存事件（由页面层订阅并调用数据源 API）
    /// </summary>
    protected Action<string, IList<object>>? OnListDataSave { get; set; }

    /// <summary>
    /// 列表数据刷新事件（由页面层订阅并调用数据源 API）
    /// </summary>
    protected Action<string>? OnListDataRefresh { get; set; }

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
}
