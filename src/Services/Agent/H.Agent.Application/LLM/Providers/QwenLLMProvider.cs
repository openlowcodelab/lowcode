using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// Qwen (通义千问) LLM Provider
/// </summary>
public class QwenLLMProvider : ILLMProvider
{
    public string ProviderName => "qwen";
    
    private readonly HttpClient _httpClient;
    
    public QwenLLMProvider(string apiKey)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
    
    public async Task<LLMResponse> ChatAsync(LLMRequest request, CancellationToken ct = default)
    {
        var payload = new
        {
            model = request.Model ?? "qwen-plus",
            input = new { messages = request.Messages },
            parameters = new
            {
                temperature = request.Temperature,
                max_tokens = request.MaxTokens
            }
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation",
            payload, ct);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<QwenResponse>(ct);
        
        return new LLMResponse
        {
            Content = result?.Output?.Text ?? string.Empty,
            Model = result?.Model ?? string.Empty,
            UsageTokens = result?.Usage?.TotalTokens ?? 0
        };
    }
    
    public async IAsyncEnumerable<string> ChatStreamAsync(LLMRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var payload = new
        {
            model = request.Model ?? "qwen-plus",
            input = new { messages = request.Messages },
            parameters = new { incremental_output = true }
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            "https://dashscope.aliyuncs.com/api/v1/services/aigc/text-generation/generation",
            payload, ct);
        
        response.EnsureSuccessStatusCode();
        
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        
        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line?.StartsWith("data:") == true)
            {
                var json = line["data:".Length..].Trim();
                if (json != "[DONE]")
                {
                    var chunk = System.Text.Json.JsonSerializer.Deserialize<QwenStreamChunk>(json);
                    var content = chunk?.Output?.Text;
                    if (!string.IsNullOrEmpty(content))
                        yield return content;
                }
            }
        }
    }
}

#region Qwen Response Types

public class QwenResponse
{
    public string Model { get; set; } = string.Empty;
    public QwenOutput Output { get; set; } = new();
    public QwenUsage? Usage { get; set; }
}

public class QwenOutput
{
    public string Text { get; set; } = string.Empty;
    public List<QwenMessage> Messages { get; set; } = new();
}

public class QwenMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class QwenUsage
{
    public int TotalTokens { get; set; }
}

public class QwenStreamChunk
{
    public QwenStreamOutput Output { get; set; } = new();
}

public class QwenStreamOutput
{
    public string Text { get; set; } = string.Empty;
}

#endregion
