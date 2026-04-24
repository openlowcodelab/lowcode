namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 环境服务配置
/// </summary>
public class EnvironmentServiceConfigDto
{
    /// <summary>
    /// 配置ID
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 环境ID
    /// </summary>
    public string EnvironmentId { get; set; } = string.Empty;

    /// <summary>
    /// 项目服务ID
    /// </summary>
    public string ProjectServiceId { get; set; } = string.Empty;

    /// <summary>
    /// 服务基础URL
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建者
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}