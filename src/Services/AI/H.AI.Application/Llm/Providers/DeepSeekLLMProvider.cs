using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace H.AI.Application;

/// <summary>
/// DeepSeek LLM Provider
/// </summary>
public class DeepSeekLLMProvider : ILLMProvider
{
    public string ProviderName => "deepseek";

    private readonly HttpClient _httpClient;
    private readonly string _defaultModel;

    public DeepSeekLLMProvider(string apiKey, string baseUrl, string model)
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

        var response = await _httpClient.PostAsJsonAsync("v1/chat/completions", payload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"DeepSeek API 返回 {(int)response.StatusCode} ({response.StatusCode}): {errorBody}",
                null,
                response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<DeepSeekResponse>(ct);
        var choice = result?.Choices?.FirstOrDefault();

        return new LLMResponse
        {
            Content = choice?.Message?.Content ?? string.Empty,
            Model = result?.Model ?? string.Empty,
            UsageTokens = result?.Usage?.TotalTokens ?? 0,
            PromptTokens = result?.Usage?.PromptTokens ?? 0,
            CompletionTokens = result?.Usage?.CompletionTokens ?? 0,
            ToolCalls = choice?.Message?.ToolCalls
        };
    }

    public async IAsyncEnumerable<LLMStreamChunk> ChatStreamAsync(LLMRequest request, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var payload = BuildPayload(request, stream: true);

        var jsonContent = payload.ToJson();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

        // 关键：使用 ResponseHeadersRead 让请求在收到响应头后立即返回，而非等待整个响应体
        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"DeepSeek API 返回 {(int)response.StatusCode} ({response.StatusCode}): {errorBody}",
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
                    var chunk = json.FromJson<DeepSeekStreamChunk>();
                    var choice = chunk?.Choices?.FirstOrDefault();

                    // usage chunk：choices 为空数组、只带 token 用量（需请求侧开启 include_usage）
                    if (choice == null)
                    {
                        if (chunk?.Usage is { } u)
                        {
                            yield return new LLMStreamChunk
                            {
                                Usage = new LLMUsage
                                {
                                    PromptTokens = u.PromptTokens,
                                    CompletionTokens = u.CompletionTokens,
                                    TotalTokens = u.TotalTokens
                                }
                            };
                        }
                        continue;
                    }

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

        // 采样参数对两种模式都必须生效：此前只写非流式分支，导致 ReAct（流式）主路径
        // 的员工人设 temperature/maxTokens 从未到达 API
        payload["temperature"] = request.Temperature;
        payload["max_tokens"] = request.MaxTokens;

        if (stream)
        {
            payload["stream"] = true;
            // 末尾 usage chunk 携带 token 用量，是 agent 运行成本账的唯一来源
            payload["stream_options"] = new Dictionary<string, object> { ["include_usage"] = true };
        }

        if (request.Tools is { Count: > 0 })
        {
            payload["tools"] = request.Tools;
        }

        return payload;
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

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class DeepSeekMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ToolCall>? ToolCalls { get; set; }
}

public class DeepSeekUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public class DeepSeekStreamChunk
{
    [JsonPropertyName("choices")]
    public List<DeepSeekStreamChoice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public DeepSeekUsage? Usage { get; set; }
}

public class DeepSeekStreamChoice
{
    [JsonPropertyName("delta")]
    public DeepSeekDelta Delta { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }
}

public class DeepSeekDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<DeepSeekStreamToolCall>? ToolCalls { get; set; }
}

public class DeepSeekStreamToolCall
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("function")]
    public DeepSeekStreamFunction? Function { get; set; }
}

public class DeepSeekStreamFunction
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}

#endregion
