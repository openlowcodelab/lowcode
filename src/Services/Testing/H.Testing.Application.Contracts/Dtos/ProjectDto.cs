using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目模型
/// </summary>
public class ProjectDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "项目名称不能为空")]
    [StringLength(20, ErrorMessage = "项目名称长度不能超过20个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "项目描述长度不能超过100个字符")]
    public string Description { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    /// <summary>
    /// 关联的知识库ID
    /// </summary>
    public string? KnowledgeBaseId { get; set; }

    public List<long> EnvironmentIds { get; set; } = new();

    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// 项目状态
/// </summary>
public enum ProjectStatus
{
    Active = 1,
    Inactive = 2,
    Completed = 3,
    Cancelled = 4,
    OnHold = 5
}