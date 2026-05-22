using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using System.Text.Json;
using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// DeepSeek LLM Provider
/// </summary>
public class DeepSeekLLMProvider : ILLMProvider
{
    public string ProviderName => "deepseek";
    
    private readonly HttpClient _httpClient;
    
    public DeepSeekLLMProvider(string apiKey, string baseUrl = "https://api.deepseek.com")
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.BaseAddress = new Uri(baseUrl);
    }
    
    public async Task<LLMResponse> ChatAsync(LLMRequest request, CancellationToken ct = default)
    {
        var payload = new
        {
            model = request.Model ?? "deepseek-chat",
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
            model = request.Model ?? "deepseek-chat",
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
                    var chunk = System.Text.Json.JsonSerializer.Deserialize<DeepSeekStreamChunk>(json);
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
    public string Model { get; set; } = string.Empty;
    public List<DeepSeekChoice> Choices { get; set; } = new();
    public DeepSeekUsage? Usage { get; set; }
}

public class DeepSeekChoice
{
    public DeepSeekMessage Message { get; set; } = new();
}

public class DeepSeekMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class DeepSeekUsage
{
    public int TotalTokens { get; set; }
}

public class DeepSeekStreamChunk
{
    public List<DeepSeekStreamChoice> Choices { get; set; } = new();
}

public class DeepSeekStreamChoice
{
    public DeepSeekDelta Delta { get; set; } = new();
}

public class DeepSeekDelta
{
    public string? Content { get; set; }
}

#endregion
