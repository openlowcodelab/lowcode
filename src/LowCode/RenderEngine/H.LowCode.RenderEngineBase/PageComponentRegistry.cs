using H.LowCode.MetaSchema.RenderEngine;

namespace H.LowCode.RenderEngineBase;

/// <summary>
/// 页面组件注册表 - 平台层通用能力
/// 渲染过程中登记页面组件树，供事件处理（保存、校验等）按 Id 查找组件配置
/// </summary>
public class PageComponentRegistry
{
    private readonly List<ComponentSchema> _roots = [];
    private readonly Dictionary<string, ComponentSchema> _componentsById = [];

    /// <summary>
    /// 登记根组件（页面直接渲染的组件）
    /// </summary>
    public void RegisterRoot(ComponentSchema component)
    {
        if (component == null)
            return;

        if (!_roots.Any(t => ReferenceEquals(t, component)))
            _roots.Add(component);

        Register(component);
    }

    /// <summary>
    /// 登记组件（含嵌套组件）
    /// </summary>
    public void Register(ComponentSchema component)
    {
        if (component == null || string.IsNullOrEmpty(component.Id))
            return;

        _componentsById[component.Id] = component;
    }

    public ComponentSchema? GetById(string componentId)
    {
        return _componentsById.TryGetValue(componentId, out var component) ? component : null;
    }

    public IReadOnlyList<ComponentSchema> GetRoots()
    {
        return _roots;
    }
}
