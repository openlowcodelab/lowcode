using H.Abp.Application.Contracts;
using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 项目 DTO
/// </summary>
public class AssistantProjectDto : AuditedEntityDto<Guid>
{
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>关联任务数（展示用）</summary>
    public int TaskCount { get; set; }
}

public class CreateAssistantProjectDto
{
    [Required(ErrorMessage = "项目名称不能为空")]
    [StringLength(200, ErrorMessage = "项目名称不能超过200个字符")]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "项目描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;
}

public class UpdateAssistantProjectDto
{
    [Required(ErrorMessage = "项目名称不能为空")]
    [StringLength(200, ErrorMessage = "项目名称不能超过200个字符")]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "项目描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;
}

public class AssistantProjectQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}
