using H.Abp.Application.Contracts;
using System.ComponentModel.DataAnnotations;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 项目 DTO
/// </summary>
public class WorkbenchProjectDto : AuditedEntityDto<Guid>
{
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>代码仓库地址（HTTPS）</summary>
    public string? RepoUrl { get; set; }

    /// <summary>默认分支</summary>
    public string? DefaultBranch { get; set; }

    /// <summary>关联任务数（展示用）</summary>
    public int TaskCount { get; set; }
}

public class CreateWorkbenchProjectDto
{
    [Required(ErrorMessage = "项目名称不能为空")]
    [StringLength(200, ErrorMessage = "项目名称不能超过200个字符")]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "项目描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "仓库地址不能超过500个字符")]
    public string? RepoUrl { get; set; }

    [StringLength(100, ErrorMessage = "默认分支不能超过100个字符")]
    public string? DefaultBranch { get; set; }
}

public class UpdateWorkbenchProjectDto
{
    [Required(ErrorMessage = "项目名称不能为空")]
    [StringLength(200, ErrorMessage = "项目名称不能超过200个字符")]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "项目描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "仓库地址不能超过500个字符")]
    public string? RepoUrl { get; set; }

    [StringLength(100, ErrorMessage = "默认分支不能超过100个字符")]
    public string? DefaultBranch { get; set; }
}

public class WorkbenchProjectQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}
