using System.ComponentModel.DataAnnotations;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 项目模型
/// </summary>
public class ProjectDto
{
    [Required(ErrorMessage = "项目标识不能为空")]
    [StringLength(12, MinimumLength = 3, ErrorMessage = "项目标识长度必须在3-12个字符之间")]
    [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9_]*$", ErrorMessage = "项目标识只能包含字母、数字、下划线，以字母开头")]
    public string Id { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "项目名称不能为空")]
    [StringLength(200, ErrorMessage = "项目名称长度不能超过200个字符")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "项目描述长度不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    
    public List<string> EnvironmentIds { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public string CreatedBy { get; set; } = "System";
    
    public string UpdatedBy { get; set; } = "System";
}

/// <summary>
/// 测试用例分类
/// </summary>
public class ProjectCaseCategory
{
    public string Id { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "分类名称不能为空")]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string ProjectId { get; set; } = string.Empty;
    
    public string ParentId { get; set; } = string.Empty; // 支持树形结构
    
    public int Order { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public string CreatedBy { get; set; } = "System";
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