namespace H.Testing.Application.Contracts;

/// <summary>
/// 环境服务配置
/// </summary>
public class ProjectEnvConfigDto
{
    /// <summary>
    /// 配置ID
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