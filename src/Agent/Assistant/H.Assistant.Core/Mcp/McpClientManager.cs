using H.Assistant.Application.Contracts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;

namespace H.Assistant.Core.Mcp;

/// <summary>
/// MCP Client 管理器 - 管理到 MCP Server 的连接和工具发现
/// </summary>
public class McpClientManager : IAsyncDisposable
{
    private readonly IMcpServerAppService _mcpServerAppService;
    private readonly ILogger<McpClientManager> _logger;
    private readonly Dictionary<string, McpClient> _clients = new();
    private readonly Dictionary<string, List<McpClientTool>> _serverTools = new();
    private bool _initialized;

    public McpClientManager(
        IMcpServerAppService mcpServerAppService,
        ILogger<McpClientManager> logger)
    {
        _mcpServerAppService = mcpServerAppService;
        _logger = logger;
    }

    /// <summary>
    /// 初始化：连接到所有已启用的 MCP Server 并发现工具
    /// </summary>
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized) return;

        try
        {
            var servers = (await _mcpServerAppService.GetAllAsync()).Data ?? [];
            var enabledServers = servers.Where(s => s.IsEnabled).ToList();

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

            _initialized = true;
            _logger.LogInformation("MCP Client 初始化完成，已连接 {Count} 个服务器", _clients.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MCP Client 初始化失败");
        }
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
                AdditionalHeaders = ParseHeaders(server.Headers)
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
        foreach (var client in _clients.Values)
        {
            try { await client.DisposeAsync(); } catch { }
        }
        _clients.Clear();
        _serverTools.Clear();
    }
}
