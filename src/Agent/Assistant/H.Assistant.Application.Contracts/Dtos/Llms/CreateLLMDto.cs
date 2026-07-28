namespace H.Assistant.Application.Contracts;

/// <summary>
/// 创建 LLM DTO
/// </summary>
public class CreateLLMDto
{
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string? ApiSecret { get; set; }
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int MaxTokens { get; set; } = 2000;
    public float Temperature { get; set; } = 0.7f;
    public int TimeoutSeconds { get; set; } = 30;
    public string? ExtraConfig { get; set; }
}
