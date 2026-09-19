namespace H.Workbench.Application.Contracts;

/// <summary>
/// LLM DTO
/// </summary>
public class LLMDto
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
    /// <summary>对外输出时为掩码（****xxxx）；仅 GetCredentialAsync 系列返回真实值</summary>
    public string ApiKey { get; set; } = string.Empty;
    /// <summary>是否已配置密钥（掩码输出下判断用）</summary>
    public bool ApiKeyConfigured { get; set; }
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
