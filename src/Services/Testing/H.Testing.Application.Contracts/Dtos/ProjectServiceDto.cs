using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目级别的服务定义
/// 定义服务的基本信息，不包含环境特定的配置
/// </summary>
public class ProjectServiceDto
{
    public long Id { get; set; }
    
    [Required(ErrorMessage = "服务名称不能为空")]
    [StringLength(100, ErrorMessage = "服务名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "服务描述长度不能超过500个字符")]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 所属项目ID
    /// </summary>
    [Required(ErrorMessage = "项目ID不能为空")]
    public long ProjectId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public string CreatedBy { get; set; } = "System";
    
    public string UpdatedBy { get; set; } = "System";
}