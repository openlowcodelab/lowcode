using H.Assistant.Application.Contracts;

namespace H.Assistant.Core;

/// <summary>
/// LLM Provider 工厂
/// </summary>
public class LLMProviderFactory
{
    private readonly ILLMAppService _configService;
    
    public LLMProviderFactory(ILLMAppService configService)
    {
        _configService = configService;
    }
    
    /// <summary>
    /// 根据 configId 创建 Provider
    /// </summary>
    public async Task<ILLMProvider?> CreateProviderAsync(Guid configId, CancellationToken ct = default)
    {
        var config = await _configService.GetAsync(configId);
        return CreateFromConfig(config);
    }
    
    /// <summary>
    /// 根据 ProviderName 创建 Provider（仅当同一 Provider 只有一个配置时可用）
    /// </summary>
    public async Task<ILLMProvider?> CreateProviderAsync(string providerName, CancellationToken ct = default)
    {
        var config = await _configService.GetConfigAsync(providerName, ct);
        return CreateFromConfig(config);
    }
    
    /// <summary>
    /// 获取默认 Provider
    /// </summary>
    public async Task<ILLMProvider?> GetDefaultProviderAsync(CancellationToken ct = default)
    {
        var defaultConfig = await _configService.GetDefaultConfigAsync(ct);
        return CreateFromConfig(defaultConfig);
    }
    
    /// <summary>
    /// 根据 config 创建 Provider
    /// </summary>
    private static ILLMProvider? CreateFromConfig(LLMDto? config)
    {
        if (config == null || !config.IsEnabled || string.IsNullOrEmpty(config.ApiKey))
            return null;
        
        return config.ProviderName.ToLowerInvariant() switch
        {
            "bailian" => new BaiLianLLMProvider(config.ApiKey, config.BaseUrl!, config.Model),
            "deepseek" => new DeepSeekLLMProvider(config.ApiKey, config.BaseUrl!, config.Model),
            _ => throw new ArgumentException($"不支持的 Provider: {config.ProviderName}")
        };
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
