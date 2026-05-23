using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// Qwen (通义千问) LLM Provider
/// </summary>
public class QwenLLMProvider : ILLMProvider
{
    public string ProviderName => "qwen";
    
    private readonly HttpClient _httpClient;
    private readonly string _defaultModel;
    
    public QwenLLMProvider(string apiKey, string? model = null)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _defaultModel = string.IsNullOrEmpty(model) ? "qwen-plus" : model;
    }
    
    public async Task<LLMResponse> ChatAsync(LLMRequest request, CancellationToken ct = default)
    {
        var payload = new
        {
            model = string.IsNullOrEmpty(request.Model) ? _defaultModel : request.Model,
            input = new { messages = request.Messages },
            parameters = new
            {
                result_format = "message",
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
            Content = result?.Output?.Choices?.FirstOrDefault()?.Message?.Content 
                     ?? result?.Output?.Text 
                     ?? string.Empty,
            Model = result?.Model ?? string.Empty,
            UsageTokens = result?.Usage?.TotalTokens ?? 0
        };
    }
    
    public async IAsyncEnumerable<string> ChatStreamAsync(LLMRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var payload = new
        {
            model = string.IsNullOrEmpty(request.Model) ? _defaultModel : request.Model,
            input = new { messages = request.Messages },
            parameters = new 
            { 
                result_format = "message",
                incremental_output = true 
            }
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
                    var chunk = json.FromJson<QwenStreamChunk>();
                    var content = chunk?.Output?.Choices?.FirstOrDefault()?.Message?.Content
                                 ?? chunk?.Output?.Text;
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
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("output")]
    public QwenOutput Output { get; set; } = new();
    
    [JsonPropertyName("usage")]
    public QwenUsage? Usage { get; set; }
}

public class QwenOutput
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("choices")]
    public List<QwenChoice> Choices { get; set; } = new();
}

public class QwenChoice
{
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public QwenMessage Message { get; set; } = new();
}

public class QwenMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class QwenUsage
{
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class QwenStreamChunk
{
    [JsonPropertyName("output")]
    public QwenStreamOutput Output { get; set; } = new();
}

public class QwenStreamOutput
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    
    [JsonPropertyName("choices")]
    public List<QwenStreamChoice> Choices { get; set; } = new();
}

public class QwenStreamChoice
{
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public QwenStreamMessage Message { get; set; } = new();
}

public class QwenStreamMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

#endregion
