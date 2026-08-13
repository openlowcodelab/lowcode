using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目环境模型
/// </summary>
public class ProjectEnvDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "环境名称不能为空")]
    [StringLength(20, ErrorMessage = "环境名称长度不能超过20个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "环境描述长度不能超过100个字符")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "项目ID不能为空")]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "环境类型不能为空")]
    public EnvironmentType Type { get; set; } = EnvironmentType.Development;

    /// <summary>
    /// 环境服务配置列表（随环境一同持久化）
    /// </summary>
    public List<ProjectEnvConfigDto> EnvironmentServiceConfigs { get; set; } = new();

    public Dictionary<string, string> Variables { get; set; } = new();

    public Dictionary<string, string> Headers { get; set; } = new();
}

/// <summary>
/// 环境服务配置（存储于环境的 ServiceConfigsJson 字段，无独立 Id，以 环境Id + 项目服务Id 定位）
/// </summary>
public class ProjectEnvConfigDto
{
    /// <summary>
    /// 配置ID（已废弃，始终为 0，仅为兼容保留）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 环境ID
    /// </summary>
    public long EnvironmentId { get; set; }

    /// <summary>
    /// 项目服务ID
    /// </summary>
    public long ProjectServiceId { get; set; }

    /// <summary>
    /// 服务基础URL
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
}

/// <summary>
/// 环境类型
/// </summary>
public enum EnvironmentType
{
    Development = 1,
    Testing = 2,
    Staging = 3,
    Production = 4
}