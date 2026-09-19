using H.Workbench.Application.Contracts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using System.Text;

namespace H.Workbench.Core.Mcp;

/// <summary>
/// MCP Client 管理器 - 管理到 MCP Server 的连接和工具发现。
/// 以启用服务器配置指纹判断是否需要（重）初始化：配置改动后无需重启进程即可生效；
/// 已知限制：删除/改名服务器后其已注册工具仍留在 ToolRegistry（调用会报错），阶段B 治理。
/// </summary>
public class McpClientManager : IAsyncDisposable
{
    private readonly IMcpServerAppService _mcpServerAppService;
    private readonly ILogger<McpClientManager> _logger;
    private readonly Dictionary<string, McpClient> _clients = new();
    private readonly Dictionary<string, List<McpClientTool>> _serverTools = new();
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private string? _lastFingerprint;

    public McpClientManager(
        IMcpServerAppService mcpServerAppService,
        ILogger<McpClientManager> logger)
    {
        _mcpServerAppService = mcpServerAppService;
        _logger = logger;
    }

    /// <summary>
    /// 初始化/按需重连：配置指纹未变则直接返回
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await _initLock.WaitAsync(ct);
        try
        {
            var servers = (await _mcpServerAppService.GetRawListAsync()).Data ?? [];
            var enabledServers = servers.Where(s => s.IsEnabled).ToList();
            var fingerprint = ComputeFingerprint(enabledServers);
            if (_lastFingerprint != null && _lastFingerprint == fingerprint) return;

            // 配置已变更：释放旧连接后全量重连
            await DisposeClientsCoreAsync();

            foreach (var server in enabledServers)
            {
                try
                {
                    await ConnectToServerAsync(server, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "连接 MCP Server {ServerName} ({Endpoint}) 失败", server.Name, server.Endpoint);
                }
            }

            _lastFingerprint = fingerprint;
            _logger.LogInformation("MCP Client 初始化完成，已连接 {Count} 个服务器", _clients.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP Client 初始化失败");
        }
        finally
        {
            _initLock.Release();
        }
    }

    private static string ComputeFingerprint(List<McpServerDto> servers)
    {
        var sb = new StringBuilder();
        foreach (var s in servers.OrderBy(s => s.Name, StringComparer.Ordinal))
        {
            sb.Append(s.Name).Append('|')
              .Append(s.Endpoint).Append('|')
              .Append(s.TransportType).Append('|')
              .Append(s.Headers).Append('|')
              .Append(s.AuthToken?.Length ?? 0).Append('|')
              .Append(s.ApiKey?.Length ?? 0).Append(';');
        }
        return sb.ToString();
    }

    /// <summary>
    /// 连接到单个 MCP Server
    /// </summary>
    private async Task ConnectToServerAsync(McpServerDto server, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(server.Endpoint))
        {
            _logger.LogWarning("MCP Server {ServerName} 的 Endpoint 为空，跳过", server.Name);
            return;
        }

        _logger.LogInformation("正在连接 MCP Server: {ServerName} ({Endpoint}, {TransportType})",
            server.Name, server.Endpoint, server.TransportType);

        var timeoutSeconds = server.TimeoutSeconds > 0 ? server.TimeoutSeconds : 30;

        IClientTransport transport = server.TransportType?.ToLowerInvariant() switch
        {
            "stdio" => new StdioClientTransport(new StdioClientTransportOptions
            {
                Command = server.Endpoint,
                Name = server.DisplayName ?? server.Name
            }),
            _ => new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(server.Endpoint),
                Name = server.DisplayName ?? server.Name,
                ConnectionTimeout = TimeSpan.FromSeconds(timeoutSeconds),
                AdditionalHeaders = BuildHeaders(server)
            })
        };

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        var client = await McpClient.CreateAsync(transport, cancellationToken: timeoutCts.Token);
        _clients[server.Name] = client;

        // 获取服务器暴露的工具
        var tools = await client.ListToolsAsync(cancellationToken: timeoutCts.Token);
        _serverTools[server.Name] = tools.ToList();

        _logger.LogInformation("MCP Server {ServerName} 已连接，发现 {ToolCount} 个工具",
            server.Name, tools.Count);
    }

    /// <summary>
    /// 组装请求头：自定义 Headers + AuthToken（Authorization: Bearer）+ ApiKey（X-API-Key）
    /// </summary>
    private static IDictionary<string, string>? BuildHeaders(McpServerDto server)
    {
        var headers = ParseHeaders(server.Headers);
        if (string.IsNullOrWhiteSpace(server.AuthToken) && string.IsNullOrWhiteSpace(server.ApiKey))
            return headers;

        var merged = headers != null
            ? new Dictionary<string, string>(headers)
            : new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(server.AuthToken))
            merged["Authorization"] = $"Bearer {server.AuthToken.Trim()}";
        if (!string.IsNullOrWhiteSpace(server.ApiKey))
            merged["X-API-Key"] = server.ApiKey.Trim();

        return merged;
    }

    /// <summary>
    /// 解析 Headers JSON 字符串为字典
    /// </summary>
    private static IDictionary<string, string>? ParseHeaders(string? headersJson)
    {
        if (string.IsNullOrWhiteSpace(headersJson)) return null;
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(headersJson);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 获取所有已发现的 MCP 工具（作为 AIFunction 列表）
    /// </summary>
    public List<AIFunction> GetAllTools()
    {
        return _serverTools.Values
            .SelectMany(t => t)
            .Cast<AIFunction>()
            .ToList();
    }

    /// <summary>
    /// 按名称查找 MCP 工具所在的服务器
    /// </summary>
    public (McpClient? client, McpClientTool? tool) FindTool(string toolName)
    {
        foreach (var (serverName, tools) in _serverTools)
        {
            var tool = tools.FirstOrDefault(t => t.Name == toolName);
            if (tool != null && _clients.TryGetValue(serverName, out var client))
            {
                return (client, tool);
            }
        }
        return (null, null);
    }

    /// <summary>
    /// 调用 MCP 工具
    /// </summary>
    public async Task<string> CallToolAsync(string toolName, IReadOnlyDictionary<string, object?>? arguments, CancellationToken ct = default)
    {
        var (client, tool) = FindTool(toolName);
        if (client == null || tool == null)
        {
            return $"MCP 工具 '{toolName}' 未找到";
        }

        try
        {
            var result = await tool.CallAsync(arguments, cancellationToken: ct);
            return result?.ToString() ?? "(无返回结果)";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "调用 MCP 工具 {ToolName} 失败", toolName);
            return $"MCP 工具调用失败: {ex.Message}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            await DisposeClientsCoreAsync();
        }
        finally
        {
            _initLock.Release();
        }
        _initLock.Dispose();
    }

    private async ValueTask DisposeClientsCoreAsync()
    {
        foreach (var client in _clients.Values)
        {
            try { await client.DisposeAsync(); } catch { }
        }
        _clients.Clear();
        _serverTools.Clear();
    }
}
