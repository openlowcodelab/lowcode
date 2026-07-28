using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// MCP 服务实体
/// </summary>
public class McpServerEntity : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// 名称（唯一标识）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 端点 URL
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// 传输方式：HTTP / SSE / Stdio
    /// </summary>
    public string TransportType { get; set; } = "SSE";

    /// <summary>
    /// Bearer Token 认证
    /// </summary>
    public string? AuthToken { get; set; }

    /// <summary>
    /// API Key 认证
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// 自定义请求头（JSON 格式）
    /// </summary>
    public string? Headers { get; set; }

    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
