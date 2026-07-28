using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 更新 MCP 服务 DTO
/// </summary>
public class UpdateMcpServerDto
{
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Endpoint { get; set; } = string.Empty;

    [StringLength(20)]
    public string TransportType { get; set; } = "SSE";

    [StringLength(500)]
    public string? AuthToken { get; set; }

    [StringLength(500)]
    public string? ApiKey { get; set; }

    [StringLength(2000)]
    public string? Headers { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public bool IsEnabled { get; set; }
}
