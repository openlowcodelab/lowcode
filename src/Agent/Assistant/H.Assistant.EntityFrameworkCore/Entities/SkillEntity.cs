using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// 技能实体
/// </summary>
public class SkillEntity : FullAuditedEntity<Guid>
{
    /// <summary>
    /// 技能名称（唯一）
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// 技能显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 技能描述（用于 AI 理解技能用途）
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 技能类型（Function/MCP/Custom）
    /// </summary>
    public string SkillType { get; set; } = "Function";

    /// <summary>
    /// 技能实现类名（对于 Function 类型）
    /// </summary>
    public string? ImplementationClass { get; set; }

    /// <summary>
    /// 技能配置（JSON 格式）
    /// </summary>
    public string? Config { get; set; }

    /// <summary>
    /// 参数定义（JSON Schema 格式）
    /// </summary>
    public string? ParameterSchema { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否需要人工审批
    /// </summary>
    public bool RequiresApproval { get; set; }

    /// <summary>
    /// 使用次数统计
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedTime { get; set; }
}
