namespace H.Assistant.Application.Contracts;

/// <summary>
/// LLM DTO
/// </summary>
public class LLMDto
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? ApiSecret { get; set; }
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
    public int TimeoutSeconds { get; set; }
    public string? ExtraConfig { get; set; }
    public DateTime CreationTime { get; set; }
}
