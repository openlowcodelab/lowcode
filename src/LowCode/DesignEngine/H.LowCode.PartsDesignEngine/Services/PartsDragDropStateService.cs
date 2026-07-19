using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.PartsDesignEngine;

/// <summary>
/// 拖拽状态服务
/// </summary>
internal static class PartsDragDropStateService
{
    #region 拖拽对象状态管理
    private static readonly IDictionary<string, DragDropStateSchema> schemaStates = new Dictionary<string, DragDropStateSchema>();
    #endregion

    public static ComponentPartsSchema GetRootComponent(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        return stateSchema?.RootComponent;
    }

    public static void SetRootComponent(string libId, string partsId, ComponentPartsSchema rootComponent)
    {
        SetStateSchema(libId, partsId, (stateSchema) => {
            stateSchema.RootComponent = rootComponent;
        });
    }

    public static PagePartsSchema GetPage(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        return stateSchema?.Page;
    }

    public static void SetPage(string libId, PagePartsSchema page)
    {
        SetStateSchema(libId, page.Id, (stateSchema) => {
            stateSchema.Page = page;
        });
    }

    public static ComponentPartsSchema GetLastSelectedComponent(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        return stateSchema?.LastSelectedComponent;
    }

    public static void SetLastSelectedComponent(string libId, string partsId, ComponentPartsSchema lastSelectedComponent)
    {
        SetStateSchema(libId, partsId, (stateSchema) => {
            stateSchema.LastSelectedComponent = lastSelectedComponent;
        });
    }

    public static ComponentPartsSchema? GetCurrentDragComponent(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        return stateSchema?.CurrentDragComponent;
    }

    public static void SetCurrentDragComponent(string libId, string partsId, ComponentPartsSchema currentDragComponent)
    {
        SetStateSchema(libId, partsId, (stateSchema) => {
            stateSchema.CurrentDragComponent = currentDragComponent;
        });
    }

    public static ComponentPartsSchema? GetLastDragOverComponent(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        return stateSchema?.LastDragOverComponent;
    }

    public static void SetLastDragOverComponent(string libId, string partsId, ComponentPartsSchema? lastDragOverComponent)
    {
        SetStateSchema(libId, partsId, (stateSchema) => {
            stateSchema.LastDragOverComponent = lastDragOverComponent;
        });
    }

    public static ComponentPartsSchema GetLastDropComponent(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        return stateSchema?.LastDropComponent;
    }

    public static void SetLastDropComponent(string libId, string partsId, ComponentPartsSchema lastDropComponent)
    {
        SetStateSchema(libId, partsId, (stateSchema) => {
            stateSchema.LastDropComponent = lastDropComponent;
        });
    }

    public static DateTime GetLastDragOverTime(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        if (stateSchema != null) return DateTime.Now;
        return stateSchema.LastDragOverTime;
    }

    public static void SetLastDragOverTime(string libId, string partsId, DateTime lastDragOverTime)
    {
        SetStateSchema(libId, partsId, (stateSchema) => {
            stateSchema.LastDragOverTime = lastDragOverTime;
        });
    }

    #region method
    private static DragDropStateSchema? GetStateSchema(string libId, string partsId)
    {
        string key = $"{libId}-{partsId}";

        if (schemaStates.TryGetValue(key, out DragDropStateSchema? schema))
        {
            return schema;
        }

        return null;
    }

    private static void SetStateSchema(string libId, string partsId, Action<DragDropStateSchema> action)
    {
        string key = $"{libId}-{partsId}";

        if (schemaStates.TryGetValue(key, out DragDropStateSchema? stateSchema))
        {
            action(stateSchema);
        }
        else
        {
            stateSchema = new();
            action(stateSchema);
            schemaStates[key] = stateSchema;
        }
    }

    public static ComponentPartsSchema FindComponentById(string libId, string partsId, string componentId)
    {
        var rootComponent = GetStateSchema(libId, partsId)?.RootComponent;
        if (rootComponent == null) return null;

        if (componentId == rootComponent.Id)
            return rootComponent;

        return FindComponentByIdRecursive(componentId, rootComponent.Childrens);
    }

    private static ComponentPartsSchema FindComponentByIdRecursive(string componentId, IList<ComponentPartsSchema> childrens)
    {
        foreach (var component in childrens)
        {
            if (component.Id == componentId) return component;

            var result = FindComponentByIdRecursive(componentId, component.Childrens);

            if (result != null) return result;
        }
        return null;
    }

    public static void ResetComponent(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        if (stateSchema == null) return;

        stateSchema.LastSelectedComponent = default;
        stateSchema.CurrentDragComponent = default;
        stateSchema.LastDragOverComponent = default;
    }

    public static void ResetDragStyle(string libId, string partsId)
    {
        var stateSchema = GetStateSchema(libId, partsId);
        if (stateSchema == null) return;

        if (stateSchema.CurrentDragComponent != null)
        {
            stateSchema.CurrentDragComponent.DesignState.AnimationTransform = string.Empty;
            stateSchema.CurrentDragComponent.DesignState.IsAnimating = false;
        }

        if (stateSchema.LastSelectedComponent != null)
        {
            stateSchema.LastSelectedComponent.DesignState.IsSelected = false;
            stateSchema.LastSelectedComponent.RefreshState();
        }

        if (stateSchema.LastDragOverComponent != null)
        {
            stateSchema.LastDragOverComponent.DesignState.DragEffectStyle = string.Empty;
            stateSchema.LastDragOverComponent.DesignState.AnimationTransform = string.Empty;
            stateSchema.LastDragOverComponent.DesignState.IsAnimating = false;
            //stateSchema.LastDragOverComponent.RefreshState();
        }

        // 重置根组件下所有子组件的动画状态
        if (stateSchema.RootComponent?.Childrens != null)
        {
            foreach (var child in stateSchema.RootComponent.Childrens)
            {
                child.DesignState.AnimationTransform = string.Empty;
                child.DesignState.IsAnimating = false;
                child.RefreshState();
            }
        }
    }
    #endregion
}

internal class DragDropStateSchema
{
    /// <summary>
    /// 根 ComponentPartsSchema
    /// </summary>
    public ComponentPartsSchema RootComponent { get; set; }

    public PagePartsSchema Page { get; set; }

    public PagePropertySchema GlobalPageProperty { get; set; } = new PagePropertySchema();

    /// <summary>
    /// 最后选中对象
    /// （当 DraggableItem 失去焦点时，即页面上没有任何项被选中，LastSelectedModel 仍有值）
    /// </summary>
    public ComponentPartsSchema LastSelectedComponent { get; set; }

    /// <summary>
    /// 当前被拖拽对象
    /// </summary>
    public ComponentPartsSchema CurrentDragComponent { get; set; }

    /// <summary>
    /// 最后一次拖拽到上面的对象
    /// </summary>
    public ComponentPartsSchema LastDragOverComponent { get; set; }

    /// <summary>
    /// 最后一次拖拽到上面的组件
    /// </summary>
    public ComponentPartsSchema LastDropComponent { get; set; }

    /// <summary>
    /// 最后一次拖拽到上面的对象的时间
    /// </summary>
    public DateTime LastDragOverTime { get; set; } = DateTime.Now;
}
