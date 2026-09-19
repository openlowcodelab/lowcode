using H.Abp.Application.Contracts;
using System.ComponentModel.DataAnnotations;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 项目资源类型常量
/// </summary>
public static class WorkbenchResourceTypes
{
    /// <summary>代码仓库</summary>
    public const string Code = "code";

    /// <summary>设计文件</summary>
    public const string Design = "design";

    /// <summary>文档</summary>
    public const string Document = "document";

    /// <summary>数据集</summary>
    public const string Data = "data";

    /// <summary>其他</summary>
    public const string Other = "other";

    public static readonly string[] All = [Code, Design, Document, Data, Other];
}

/// <summary>
/// 项目资源 DTO
/// </summary>
public class WorkbenchProjectResourceDto
{
    /// <summary>资源类型（见 <see cref="WorkbenchResourceTypes"/>）</summary>
    public string ResourceType { get; set; } = WorkbenchResourceTypes.Other;

    /// <summary>资源名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>资源地址（仓库 URL、文件链接等）</summary>
    public string? Url { get; set; }

    /// <summary>默认分支（仅 code 类型有效）</summary>
    public string? Branch { get; set; }
}

/// <summary>
/// 项目资源输入（新建/编辑项目时全量提交）
/// </summary>
public class WorkbenchProjectResourceInputDto
{
    [Required(ErrorMessage = "资源类型不能为空")]
    public string ResourceType { get; set; } = WorkbenchResourceTypes.Code;

    [Required(ErrorMessage = "资源名称不能为空")]
    [StringLength(200, ErrorMessage = "资源名称不能超过200个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "资源地址不能超过500个字符")]
    public string? Url { get; set; }

    [StringLength(100, ErrorMessage = "默认分支不能超过100个字符")]
    public string? Branch { get; set; }
}

/// <summary>
/// 项目 DTO
/// </summary>
public class WorkbenchProjectDto : AuditedEntityDto<Guid>
{
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>关联资源（代码仓库、设计文件等）</summary>
    public List<WorkbenchProjectResourceDto> Resources { get; set; } = [];

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

    /// <summary>资源列表（保存时整体写入）</summary>
    public List<WorkbenchProjectResourceInputDto> Resources { get; set; } = [];
}

public class UpdateWorkbenchProjectDto
{
    [Required(ErrorMessage = "项目名称不能为空")]
    [StringLength(200, ErrorMessage = "项目名称不能超过200个字符")]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "项目描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    /// <summary>资源列表（保存时全量替换）</summary>
    public List<WorkbenchProjectResourceInputDto> Resources { get; set; } = [];
}

public class WorkbenchProjectQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}
