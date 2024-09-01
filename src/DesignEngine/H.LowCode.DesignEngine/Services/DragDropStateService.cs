using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine;

/// <summary>
/// 拖拽状态服务
/// </summary>
internal class DragDropStateService
{
    #region 拖拽对象状态管理
    private IDictionary<string, DragDropStateSchema> schemaStates = new Dictionary<string, DragDropStateSchema>();
    #endregion

    public ComponentSchema GetRootComponent(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        return stateSchema?.RootComponent;
    }

    public void SetRootComponent(string appId, string pageId, ComponentSchema rootComponent)
    {
        SetStateSchema(appId, pageId, (stateSchema) => {
            stateSchema.RootComponent = rootComponent;
        });
    }

    public PageSchema GetPage(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        return stateSchema?.Page;
    }

    public void SetPage(string appId, PageSchema page)
    {
        SetStateSchema(appId, page.Id, (stateSchema) => {
            stateSchema.Page = page;
        });
    }

    public PagePropertySchema GetGlobalPageProperty(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        return stateSchema?.GlobalPageProperty;
    }

    public void SetGlobalPageProperty(string appId, string pageId, PagePropertySchema globalPageProperty)
    {
        SetStateSchema(appId, pageId, (stateSchema) => {
            stateSchema.GlobalPageProperty = globalPageProperty;
        });
    }

    public ComponentSchema GetLastSelectedComponent(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        return stateSchema?.LastSelectedComponent;
    }

    public void SetLastSelectedComponent(string appId, string pageId, ComponentSchema lastSelectedComponent)
    {
        SetStateSchema(appId, pageId, (stateSchema) => {
            stateSchema.LastSelectedComponent = lastSelectedComponent;
        });
    }

    public ComponentSchema GetCurrentDragComponent(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        return stateSchema?.CurrentDragComponent;
    }

    public void SetCurrentDragComponent(string appId, string pageId, ComponentSchema currentDragComponent)
    {
        SetStateSchema(appId, pageId, (stateSchema) => {
            stateSchema.CurrentDragComponent = currentDragComponent;
        });
    }

    public ComponentSchema GetLastDragOverComponent(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        return stateSchema?.LastDragOverComponent;
    }

    public void SetLastDragOverComponent(string appId, string pageId, ComponentSchema lastDragOverComponent)
    {
        SetStateSchema(appId, pageId, (stateSchema) => {
            stateSchema.LastDragOverComponent = lastDragOverComponent;
        });
    }

    public ComponentSchema GetLastDropComponent(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        return stateSchema?.LastDropComponent;
    }

    public void SetLastDropComponent(string appId, string pageId, ComponentSchema lastDropComponent)
    {
        SetStateSchema(appId, pageId, (stateSchema) => {
            stateSchema.LastDropComponent = lastDropComponent;
        });
    }

    public DateTime GetLastDragOverTime(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        if (stateSchema != null) return DateTime.Now;
        return stateSchema.LastDragOverTime;
    }

    public void SetLastDragOverTime(string appId, string pageId, DateTime lastDragOverTime)
    {
        SetStateSchema(appId, pageId, (stateSchema) => {
            stateSchema.LastDragOverTime = lastDragOverTime;
        });
    }

    #region method
    private DragDropStateSchema GetStateSchema(string appId, string pageId)
    {
        string key = $"{appId}-{pageId}";

        if (schemaStates.TryGetValue(key, out DragDropStateSchema schema))
            return schema;

        return null;
    }

    private void SetStateSchema(string appId, string pageId, Action<DragDropStateSchema> action)
    {
        string key = $"{appId}-{pageId}";

        if (schemaStates.TryGetValue(key, out DragDropStateSchema stateSchema))
            action(stateSchema);
        else
        {
            stateSchema = new();
            action(stateSchema);
            schemaStates[key] = stateSchema;
        }
    }

    public ComponentSchema FindComponentById(string appId, string pageId, Guid componentId)
    {
        var rootComponent = GetStateSchema(appId, pageId)?.RootComponent;
        if (rootComponent == null) return null;

        if(componentId == rootComponent.Id)
            return rootComponent;

        return FindComponentByIdRecursive(componentId, rootComponent.Childrens);
    }

    private ComponentSchema FindComponentByIdRecursive(Guid componentId, IList<ComponentSchema> childrens)
    {
        foreach (var component in childrens)
        {
            if (component.Id == componentId) return component;

            var result = FindComponentByIdRecursive(componentId, component.Childrens);

            if (result != null) return result;
        }
        return null;
    }

    public void ResetComponent(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        if (stateSchema == null) return;

        stateSchema.LastSelectedComponent = default;
        stateSchema.CurrentDragComponent = default;
        stateSchema.LastDragOverComponent = default;
    }

    public void ResetDragStyle(string appId, string pageId)
    {
        var stateSchema = GetStateSchema(appId, pageId);
        if (stateSchema == null) return;

        if (stateSchema.CurrentDragComponent != null)
        {
            stateSchema.CurrentDragComponent.Opacity = 1;
        }

        if (stateSchema.LastSelectedComponent != null)
        {
            stateSchema.LastSelectedComponent.IsSelected = false;
            stateSchema.LastSelectedComponent.RefreshState();
        }

        if (stateSchema.LastDragOverComponent != null)
        {
            stateSchema.LastDragOverComponent.DragEffectStyle = string.Empty;
            //stateSchema.LastDragOverComponent.RefreshState();
        }
    }
    #endregion
}

public class DragDropStateSchema
{
    /// <summary>
    /// 根 ComponentSchema
    /// </summary>
    public ComponentSchema RootComponent { get; set; }

    public PageSchema Page { get; set; }

    public PagePropertySchema GlobalPageProperty { get; set; } = new PagePropertySchema();

    /// <summary>
    /// 最后选中对象
    /// （当 DropItem 失去焦点时，即页面上没有任何项被选中，LastSelectedModel 仍有值）
    /// </summary>
    public ComponentSchema LastSelectedComponent { get; set; }

    /// <summary>
    /// 当前被拖拽对象
    /// </summary>
    public ComponentSchema CurrentDragComponent { get; set; }

    /// <summary>
    /// 最后一次拖拽到上面的对象
    /// </summary>
    public ComponentSchema LastDragOverComponent { get; set; }

    /// <summary>
    /// 最后一次拖拽到上面的组件
    /// </summary>
    public ComponentSchema LastDropComponent { get; set; }

    /// <summary>
    /// 最后一次拖拽到上面的对象的时间
    /// </summary>
    public DateTime LastDragOverTime { get; set; } = DateTime.Now;
}
