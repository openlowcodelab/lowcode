using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// LLM 配置实体
/// </summary>
public class LLMConfigEntity : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// 厂商名称
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;
    
    /// <summary>
    /// 显示名称
    /// </summary>
    public string ProviderDisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// API Key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// API Secret
    /// </summary>
    public string? ApiSecret { get; set; }
    
    /// <summary>
    /// 基础 URL
    /// </summary>
    public string? BaseUrl { get; set; }
    
    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 是否为默认 Provider
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// 最大 Token 数
    /// </summary>
    public int MaxTokens { get; set; }
    
    /// <summary>
    /// 温度参数
    /// </summary>
    public float Temperature { get; set; }
    
    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; }
    
    /// <summary>
    /// 额外配置 JSON
    /// </summary>
    public string? ExtraConfig { get; set; }
}
