using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 插件实体（一组技能打包成的能力扩展，对齐数字员工系统的插件概念）
/// </summary>
public class WorkbenchPluginEntity : AuditedEntity<Guid>
{
    /// <summary>
    /// 插件标识（唯一，如 dev_engineering）
    /// </summary>
    public string PluginKey { get; set; } = string.Empty;

    /// <summary>
    /// 插件名称
    /// </summary>
    public string PluginName { get; set; } = string.Empty;

    /// <summary>
    /// 插件描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 来源：Market(插件市场)/Custom(自定义)
    /// </summary>
    public string Source { get; set; } = "Custom";

    /// <summary>
    /// 图标（emoji 或标识文本）
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 授予的技能名列表（JSON 数组，对应 WorkbenchToolCatalog 的技能键）
    /// </summary>
    public string? SkillKeys { get; set; }
}
