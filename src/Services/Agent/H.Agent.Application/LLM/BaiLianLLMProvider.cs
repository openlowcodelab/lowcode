using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using H.Agent.Application.Contracts;

namespace H.Agent.Application;

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
        var payload = new
        {
            model = string.IsNullOrEmpty(request.Model) ? _defaultModel : request.Model,
            messages = request.Messages,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };
        
        var response = await _httpClient.PostAsJsonAsync("chat/completions", payload, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"DashScope API 返回 {(int)response.StatusCode} ({response.StatusCode}): {errorBody}");
        }
        
        var result = await response.Content.ReadFromJsonAsync<QwenResponse>(ct);
        
        return new LLMResponse
        {
            Content = result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty,
            Model = result?.Model ?? string.Empty,
            UsageTokens = result?.Usage?.TotalTokens ?? 0
        };
    }
    
    public async IAsyncEnumerable<string> ChatStreamAsync(LLMRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var payload = new
        {
            model = string.IsNullOrEmpty(request.Model) ? _defaultModel : request.Model,
            messages = request.Messages,
            stream = true
        };
        
        var jsonContent = payload.ToJson();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));
        
        // 关键：使用 ResponseHeadersRead 让请求在收到响应头后立即返回，而非等待整个响应体
        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"DashScope API 返回 {(int)response.StatusCode} ({response.StatusCode}): {errorBody}");
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
                    var content = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
                    if (!string.IsNullOrEmpty(content))
                        yield return content;
                }
            }
        }
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
    [JsonPropertyName("choices")]
    public List<QwenStreamChoice> Choices { get; set; } = new();
}

public class QwenStreamChoice
{
    [JsonPropertyName("delta")]
    public QwenStreamDelta Delta { get; set; } = new();
}

public class QwenStreamDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

#endregion
