using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目环境模型
/// </summary>
public class ProjectEnvironmentDto
{
    public long Id { get; set; }
    
    [Required(ErrorMessage = "环境名称不能为空")]
    [StringLength(100, ErrorMessage = "环境名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "环境描述长度不能超过500个字符")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "项目ID不能为空")]
    public long ProjectId { get; set; }
    
    [Required(ErrorMessage = "环境类型不能为空")]
    public EnvironmentType Type { get; set; }
    
    /// <summary>
    /// 环境服务配置列表
    /// </summary>
    public List<EnvironmentServiceConfigDto> EnvironmentServiceConfigs { get; set; } = new();
    
    public Dictionary<string, string> Variables { get; set; } = new();
    
    public Dictionary<string, string> Headers { get; set; } = new();
    
    public DatabaseConfig? DatabaseConfig { get; set; }
    
    public EnvironmentStatus Status { get; set; } = EnvironmentStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
    public string CreatedBy { get; set; } = "System";
    
    public string UpdatedBy { get; set; } = "System";
}

/// <summary>
/// 数据库配置
/// </summary>
public class DatabaseConfig
{
    public string ConnectionString { get; set; } = string.Empty;
    
    public DatabaseType Type { get; set; }
    
    public string Host { get; set; } = string.Empty;
    
    public int Port { get; set; }
    
    public string Database { get; set; } = string.Empty;
    
    public string Username { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
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



/// <summary>
/// 数据库类型
/// </summary>
public enum DatabaseType
{
    SqlServer = 1,
    MySQL = 2,
    PostgreSQL = 3,
    Oracle = 4,
    SQLite = 5
}