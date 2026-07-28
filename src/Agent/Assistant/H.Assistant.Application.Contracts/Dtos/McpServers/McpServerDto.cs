namespace H.Assistant.Application.Contracts;

/// <summary>
/// MCP 服务 DTO
/// </summary>
public class McpServerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string TransportType { get; set; } = "SSE";
    public string? AuthToken { get; set; }
    public string? ApiKey { get; set; }
    public string? Headers { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime CreationTime { get; set; }
}
