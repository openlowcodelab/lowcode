using System.ComponentModel.DataAnnotations;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试环境模型
/// </summary>
public class EnvironmentDto
{
    public long Id { get; set; }

    [Required(ErrorMessage = "环境名称不能为空")]
    [StringLength(20, ErrorMessage = "环境名称长度不能超过20个字符")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "环境描述长度不能超过100个字符")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "所属项目不能为空")]
    public long ProjectId { get; set; }

    /// <summary>
    /// 环境配置，包含各种环境变量和配置信息
    /// </summary>
    public Dictionary<string, object> Config { get; set; } = new();

    /// <summary>
    /// 服务端点配置（按服务ID索引）
    /// </summary>
    public Dictionary<long, string> ServiceEndpoints { get; set; } = new();

    /// <summary>
    /// 排序字段，数值越小排序越靠前
    /// </summary>
    public int Order { get; set; }
}