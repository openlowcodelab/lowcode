using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using H.Agent.Application.Contracts;
using System;

namespace H.Agent.Application;

/// <summary>
/// DeepSeek LLM Provider
/// </summary>
public class DeepSeekLLMProvider : ILLMProvider
{
    public string ProviderName => "deepseek";
    
    private readonly HttpClient _httpClient;
    private readonly string _defaultModel;
    
    public DeepSeekLLMProvider(string apiKey, string? baseUrl = "https://api.deepseek.com", string? model = null)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.BaseAddress = new Uri(string.IsNullOrEmpty(baseUrl) ? "https://api.deepseek.com" : baseUrl);
        _defaultModel = string.IsNullOrEmpty(model) ? "deepseek-chat" : model;
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
        
        var response = await _httpClient.PostAsJsonAsync("/v1/chat/completions", payload, ct);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<DeepSeekResponse>(ct);
        
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
        
        var response = await _httpClient.PostAsJsonAsync("/v1/chat/completions", payload, ct);
        response.EnsureSuccessStatusCode();
        
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
                    var chunk = json.FromJson<DeepSeekStreamChunk>();
                    var content = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
                    if (!string.IsNullOrEmpty(content))
                        yield return content;
                }
            }
        }
    }
}

#region DeepSeek Response Types

public class DeepSeekResponse
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;
    
    [JsonPropertyName("choices")]
    public List<DeepSeekChoice> Choices { get; set; } = new();
    
    [JsonPropertyName("usage")]
    public DeepSeekUsage? Usage { get; set; }
}

public class DeepSeekChoice
{
    [JsonPropertyName("message")]
    public DeepSeekMessage Message { get; set; } = new();
}

public class DeepSeekMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class DeepSeekUsage
{
    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class DeepSeekStreamChunk
{
    [JsonPropertyName("choices")]
    public List<DeepSeekStreamChoice> Choices { get; set; } = new();
}

public class DeepSeekStreamChoice
{
    [JsonPropertyName("delta")]
    public DeepSeekDelta Delta { get; set; } = new();
}

public class DeepSeekDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}

#endregion
