using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace H.Assistant.Core;

/// <summary>
/// 阿里云百炼 LLM Provider
/// </summary>
public class BaiLianLLMProvider : ILLMProvider
{
    public string ProviderName => "qwen";
    
    private readonly HttpClient _httpClient;
    private readonly string _defaultModel;
    
    public BaiLianLLMProvider(string apiKey, string baseUrl, string model)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        // 确保 BaseAddress 以 '/' 结尾，避免相对路径拼接时丢失 BaseUrl 中的路径部分
        _httpClient.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        _defaultModel = model;
    }
    
    public async Task<LLMResponse> ChatAsync(LLMRequest request, CancellationToken ct = default)
    {
        var payload = BuildPayload(request, stream: false);
        
        var response = await _httpClient.PostAsJsonAsync("chat/completions", payload, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"DashScope API 返回 {(int)response.StatusCode} ({response.StatusCode}): {errorBody}",
                null,
                response.StatusCode);
        }
        
        var result = await response.Content.ReadFromJsonAsync<QwenResponse>(ct);
        var choice = result?.Choices?.FirstOrDefault();
        
        return new LLMResponse
        {
            Content = choice?.Message?.Content ?? string.Empty,
            Model = result?.Model ?? string.Empty,
            UsageTokens = result?.Usage?.TotalTokens ?? 0,
            ToolCalls = choice?.Message?.ToolCalls
        };
    }
    
    public async IAsyncEnumerable<LLMStreamChunk> ChatStreamAsync(LLMRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var payload = BuildPayload(request, stream: true);
        
        var jsonContent = payload.ToJson();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        
        // 关键：使用 ResponseHeadersRead 让请求在收到响应头后立即返回，而非等待整个响应体
        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"DashScope API 返回 {(int)response.StatusCode} ({response.StatusCode}): {errorBody}",
                null,
                response.StatusCode);
        }
        
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        
        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);
            if (line?.StartsWith("data: ") == true)
            {
                var json = line["data: ".Length..];
                if (json != "[DONE]")
                {
                    var chunk = json.FromJson<QwenStreamChunk>();
                    var choice = chunk?.Choices?.FirstOrDefault();
                    if (choice == null) continue;
                    
                    var streamChunk = new LLMStreamChunk
                    {
                        Content = choice.Delta?.Content,
                        FinishReason = choice.FinishReason
                    };
                    
                    // 流式 tool_calls 增量
                    if (choice.Delta?.ToolCalls is { Count: > 0 })
                    {
                        var tc = choice.Delta.ToolCalls[0];
                        streamChunk.ToolCallDelta = new ToolCallDelta
                        {
                            Index = tc.Index,
                            Id = tc.Id,
                            FunctionName = tc.Function?.Name,
                            FunctionArgumentsDelta = tc.Function?.Arguments
                        };
                    }
                    
                    yield return streamChunk;
                }
            }
        }
    }
    
    private object BuildPayload(LLMRequest request, bool stream)
    {
        var payload = new Dictionary<string, object>
        {
            ["model"] = string.IsNullOrEmpty(request.Model) ? _defaultModel : request.Model,
            ["messages"] = request.Messages
        };
        
        if (!stream)
        {
            payload["temperature"] = request.Temperature;
            payload["max_tokens"] = request.MaxTokens;
        }
        else
        {
            payload["stream"] = true;
        }
        
        if (request.Tools is { Count: > 0 })
        {
            payload["tools"] = request.Tools;
        }
        
        return payload;
    }
}

#region Qwen Response Types (OpenAI 兼容格式)

public class QwenResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("choices")]
    public List<QwenChoice> Choices { get; set; } = new();
    
    [JsonPropertyName("usage")]
    public QwenUsage? Usage { get; set; }
}

public class QwenChoice
{
    [JsonPropertyName("message")]
    public QwenMessage Message { get; set; } = new();
    
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class QwenMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ToolCall>? ToolCalls { get; set; }
}

public class QwenUsage
{
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class QwenStreamChunk
{
    [JsonPropertyName("choices")]
    public List<QwenStreamChoice> Choices { get; set; } = new();
}

public class QwenStreamChoice
{
    [JsonPropertyName("delta")]
    public QwenStreamDelta Delta { get; set; } = new();
    
    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class QwenStreamDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<QwenStreamToolCall>? ToolCalls { get; set; }
}

public class QwenStreamToolCall
{
    [JsonPropertyName("index")]
    public int Index { get; set; }
    
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("function")]
    public QwenStreamFunction? Function { get; set; }
}

public class QwenStreamFunction
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}

#endregion
