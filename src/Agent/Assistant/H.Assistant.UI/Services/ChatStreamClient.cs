using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using H.Abp.HttpClientProxy;
using H.Assistant.Application.Contracts;

namespace H.Assistant.UI.Services;

/// <summary>
/// 聊天 SSE 流式客户端，对接 /api/assistant/chat/stream（与 Web 端 JS fetch 流实现一致）
/// </summary>
public class ChatStreamClient(IHttpClientFactory httpClientFactory, RemoteServiceOptions remoteServiceOptions)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 发送消息并逐条产出 SSE data 负载（不含 "data: " 前缀，遇 [DONE] 结束）
    /// </summary>
    public async IAsyncEnumerable<string> StreamAsync(SendChatMessageInputDto input, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(AssistantApp.AssistantRemoteServiceName);
        var baseUrl = remoteServiceOptions.GetBaseUrl(AssistantApp.AssistantRemoteServiceName).TrimEnd('/');
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/assistant/chat/stream")
        {
            Content = JsonContent.Create(input, options: JsonOptions)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line == null)
            {
                yield break;
            }

            if (!line.StartsWith("data: ", StringComparison.Ordinal))
            {
                continue;
            }

            var data = line[6..];
            if (data == "[DONE]")
            {
                yield break;
            }

            yield return data;
        }
    }
}
