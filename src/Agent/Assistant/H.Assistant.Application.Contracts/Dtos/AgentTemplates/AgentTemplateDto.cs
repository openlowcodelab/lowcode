using H.Abp.Application.Contracts;
using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 员工模板 DTO
/// </summary>
public class AgentTemplateDto : AuditedEntityDto<Guid>
{
    public string TemplateName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public List<Guid> SkillIds { get; set; } = new();
    public List<Guid> ConnectorIds { get; set; } = new();
    public List<Guid> KnowledgeBaseIds { get; set; } = new();
    public List<Guid> ProjectIds { get; set; } = new();
    public bool IsBuiltin { get; set; }
}

public class AgentTemplateQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}

/// <summary>
/// 从模板新建员工输入
/// </summary>
public class CreateAgentFromTemplateDto
{
    [Required(ErrorMessage = "模板不能为空")]
    public Guid TemplateId { get; set; }

    [Required(ErrorMessage = "员工名称不能为空")]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(100)]
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// 员工保存为模板输入
/// </summary>
public class SaveAgentTemplateDto
{
    [Required(ErrorMessage = "员工不能为空")]
    public Guid AgentId { get; set; }

    [Required(ErrorMessage = "模板名称不能为空")]
    [StringLength(200)]
    public string TemplateName { get; set; } = string.Empty;
}
