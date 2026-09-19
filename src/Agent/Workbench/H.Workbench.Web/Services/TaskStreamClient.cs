using H.Abp.HttpClientProxy;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace H.Workbench.Web.Services;

/// <summary>
/// 任务流式执行 SSE 客户端，对接 /api/workbench/task/stream（与桌面端 ChatStreamClient 模式一致）
/// </summary>
public class TaskStreamClient
{
    private const string RemoteServiceName = "Workbench";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RemoteServiceOptions _remoteServiceOptions;

    public TaskStreamClient(IHttpClientFactory httpClientFactory, RemoteServiceOptions remoteServiceOptions)
    {
        _httpClientFactory = httpClientFactory;
        _remoteServiceOptions = remoteServiceOptions;
    }

    /// <summary>
    /// 流式执行任务，逐条产出 SSE data 负载（不含 "data: " 前缀，遇 [DONE] 结束）
    /// </summary>
    public async IAsyncEnumerable<string> StreamAsync(Guid taskId, string? prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient(RemoteServiceName);
        // Agent 执行（含工具调用）可能远超默认 100s 超时，交由 cancellationToken 控制
        client.Timeout = Timeout.InfiniteTimeSpan;

        var baseUrl = _remoteServiceOptions.GetBaseUrl(RemoteServiceName).TrimEnd('/');
        using var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/workbench/task/stream")
        {
            Content = JsonContent.Create(new { taskId, prompt }, options: JsonOptions)
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
