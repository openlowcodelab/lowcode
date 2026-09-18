using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 员工模板实体（用于从模板新建员工）
/// </summary>
public class AgentTemplateEntity : AuditedEntity<Guid>
{
    /// <summary>
    /// 模板名称
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// 角色（如：研发工程师）
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 系统提示词
    /// </summary>
    public string SystemPrompt { get; set; } = string.Empty;

    /// <summary>
    /// 关联的技能 ID 列表（JSON 格式）
    /// </summary>
    public string? SkillIds { get; set; }

    /// <summary>
    /// 关联的连接器 ID 列表（JSON 格式）
    /// </summary>
    public string? ConnectorIds { get; set; }

    /// <summary>
    /// 关联的知识库 ID 列表（JSON 格式）
    /// </summary>
    public string? KnowledgeBaseIds { get; set; }

    /// <summary>
    /// 关联的项目 ID 列表（JSON 格式）
    /// </summary>
    public string? ProjectIds { get; set; }

    /// <summary>
    /// 是否内置模板
    /// </summary>
    public bool IsBuiltin { get; set; }
}
