using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// LLM Provider 工厂
/// </summary>
public class LLMProviderFactory
{
    private readonly ILLMConfigAppService _configService;
    
    public LLMProviderFactory(ILLMConfigAppService configService)
    {
        _configService = configService;
    }
    
    /// <summary>
    /// 创建指定 Provider
    /// </summary>
    public async Task<ILLMProvider?> CreateProviderAsync(string providerName, CancellationToken ct = default)
    {
        var config = await _configService.GetConfigAsync(providerName, ct);
        if (config == null || !config.IsEnabled || string.IsNullOrEmpty(config.ApiKey))
            return null;
        
        return providerName.ToLowerInvariant() switch
        {
            "deepseek" => new DeepSeekLLMProvider(config.ApiKey, config.BaseUrl),
            "qwen" => new QwenLLMProvider(config.ApiKey),
            _ => throw new ArgumentException($"不支持的 Provider: {providerName}")
        };
    }
    
    /// <summary>
    /// 获取默认 Provider
    /// </summary>
    public async Task<ILLMProvider?> GetDefaultProviderAsync(CancellationToken ct = default)
    {
        var defaultConfig = await _configService.GetDefaultConfigAsync(ct);
        if (defaultConfig == null)
            return null;
        
        return await CreateProviderAsync(defaultConfig.ProviderName, ct);
    }
    
    /// <summary>
    /// 获取所有可用的 Provider 名称
    /// </summary>
    public async Task<List<string>> GetAvailableProvidersAsync(CancellationToken ct = default)
    {
        var configs = await _configService.GetAllAsync();
        return configs
            .Where(c => c.IsEnabled && !string.IsNullOrEmpty(c.ApiKey))
            .Select(c => c.ProviderName)
            .ToList();
    }
}
