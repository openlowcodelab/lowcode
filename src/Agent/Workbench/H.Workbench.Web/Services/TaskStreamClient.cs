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
    /// 提交一次后台运行，返回 runId（= TaskLog 主键）。
    /// 执行不再挂在这条连接上，所以可以先拿 id、再决定要不要看。
    /// </summary>
    public async Task<Guid> StartAsync(Guid taskId, string? prompt, CancellationToken ct = default)
    {
        var client = CreateClient();
        var baseUrl = _remoteServiceOptions.GetBaseUrl(RemoteServiceName).TrimEnd('/');

        var response = await client.PostAsJsonAsync($"{baseUrl}/api/workbench/task/start",
            new { taskId, prompt }, JsonOptions, ct);
        response.EnsureSuccessStatusCode();

        // BaseOutput<Guid> 包裹
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        return doc.RootElement.TryGetProperty("data", out var data) && data.TryGetGuid(out var runId)
            ? runId
            : Guid.Empty;
    }

    /// <summary>
    /// 订阅某次运行的事件流（先回放缓冲再接实时）。刷新或切走再回来时用这个续上。
    /// </summary>
    public IAsyncEnumerable<string> SubscribeAsync(Guid runId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        => ReadSseAsync(HttpMethod.Get, $"{Base()}/api/workbench/task/stream/{runId}", null, cancellationToken);

    public async Task CancelAsync(Guid runId, CancellationToken ct = default)
    {
        var client = CreateClient();
        await client.PostAsync($"{Base()}/api/workbench/task/cancel/{runId}", new StringContent(""), ct);
    }

    /// <summary>
    /// 流式执行任务，逐条产出 SSE data 负载（不含 "data: " 前缀，遇 [DONE] 结束）
    /// </summary>
    public async IAsyncEnumerable<string> StreamAsync(Guid taskId, string? prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var data in ReadSseAsync(HttpMethod.Post, $"{Base()}/api/workbench/task/stream",
                           JsonContent.Create(new { taskId, prompt }, options: JsonOptions), cancellationToken))
        {
            yield return data;
        }
    }

    private string Base() => _remoteServiceOptions.GetBaseUrl(RemoteServiceName).TrimEnd('/');

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient(RemoteServiceName);
        // Agent 执行（含工具调用）可能远超默认 100s 超时，交由 cancellationToken 控制
        client.Timeout = Timeout.InfiniteTimeSpan;
        return client;
    }

    private async IAsyncEnumerable<string> ReadSseAsync(HttpMethod method, string url, HttpContent? content,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var client = CreateClient();
        using var request = new HttpRequestMessage(method, url);
        if (content is not null)
        {
            request.Content = content;
        }
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
