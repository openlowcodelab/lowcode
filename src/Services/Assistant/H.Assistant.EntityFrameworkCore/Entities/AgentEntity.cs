using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// Agent 实体
/// </summary>
public class AgentEntity : FullAuditedEntity<Guid>
{
    /// <summary>
    /// Agent 类型标识（唯一）
    /// </summary>
    public string AgentType { get; set; } = string.Empty;

    /// <summary>
    /// Agent 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Agent 描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 是否支持流式响应
    /// </summary>
    public bool SupportsStreaming { get; set; } = true;

    /// <summary>
    /// 温度参数（0-1）
    /// </summary>
    public float Temperature { get; set; } = 0.7f;

    /// <summary>
    /// 最大 Token 数
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// 默认模型配置 ID（可选）
    /// </summary>
    public Guid? DefaultModelConfigId { get; set; }

    /// <summary>
    /// Agent 元数据（JSON 格式）
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// 关联的技能 ID 列表（JSON 格式）
    /// </summary>
    public string? SkillIds { get; set; }
}
