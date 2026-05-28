namespace H.Assistant.Core;

/// <summary>
/// LLM Provider 统一接口
/// </summary>
public interface ILLMProvider
{
    /// <summary>
    /// Provider 名称
    /// </summary>
    string ProviderName { get; }
    
    /// <summary>
    /// 同步对话
    /// </summary>
    Task<LLMResponse> ChatAsync(LLMRequest request, CancellationToken ct = default);
    
    /// <summary>
    /// 流式对话
    /// </summary>
    IAsyncEnumerable<string> ChatStreamAsync(LLMRequest request, CancellationToken ct = default);
}
