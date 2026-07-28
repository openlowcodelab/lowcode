namespace H.Assistant.Application.Contracts;

/// <summary>
/// 更新 LLM DTO
/// </summary>
public class UpdateLLMDto
{
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public int MaxTokens { get; set; }
    public float Temperature { get; set; }
    public int TimeoutSeconds { get; set; }
    public string? ExtraConfig { get; set; }
}
