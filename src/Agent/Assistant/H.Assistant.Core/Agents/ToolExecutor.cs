using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace H.Assistant.Core.Agents;

/// <summary>
/// 工具执行器 - 负责查找、解析参数并执行工具
/// </summary>
public class ToolExecutor
{
    private readonly IToolRegistry _toolRegistry;
    private readonly ILogger<ToolExecutor> _logger;

    /// <summary>
    /// 工具结果最大字符数，超出则截断
    /// </summary>
    private const int MaxResultLength = 4000;

    /// <summary>
    /// 单次工具执行超时（秒）
    /// </summary>
    private const int ExecutionTimeoutSeconds = 60;

    public ToolExecutor(IToolRegistry toolRegistry, ILogger<ToolExecutor> logger)
    {
        _toolRegistry = toolRegistry;
        _logger = logger;
    }

    /// <summary>
    /// 执行工具调用
    /// </summary>
    public async Task<(string result, bool isError)> ExecuteAsync(
        string toolName,
        string argumentsJson,
        CancellationToken ct = default)
    {
        var tool = _toolRegistry.GetTool(toolName);
        if (tool == null)
        {
            return ($"工具 '{toolName}' 未找到。可用工具: {string.Join(", ", _toolRegistry.GetAllTools().Select(t => t.Name))}", true);
        }

        try
        {
            var arguments = ParseArguments(argumentsJson);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(ExecutionTimeoutSeconds));

            _logger.LogInformation("执行工具: {ToolName}, 参数: {Args}", toolName,
                argumentsJson.Length > 200 ? argumentsJson[..200] + "..." : argumentsJson);

            var result = await tool.InvokeAsync(
                arguments != null ? new AIFunctionArguments(arguments) : null,
                timeoutCts.Token);

            var resultText = result?.ToString() ?? "(无返回结果)";

            // 截断过长的结果
            if (resultText.Length > MaxResultLength)
            {
                resultText = resultText[..MaxResultLength] + "\n...[结果已截断]";
            }

            _logger.LogInformation("工具 {ToolName} 执行成功, 结果长度: {Len}", toolName, resultText.Length);
            return (resultText, false);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            var msg = $"工具 '{toolName}' 执行超时（{ExecutionTimeoutSeconds}秒）";
            _logger.LogWarning(msg);
            return (msg, true);
        }
        catch (Exception ex)
        {
            var msg = $"工具 '{toolName}' 执行失败: {ex.Message}";
            _logger.LogWarning(ex, "工具 {ToolName} 执行异常", toolName);
            return (msg, true);
        }
    }

    /// <summary>
    /// 解析 JSON 参数为字典
    /// </summary>
    private static Dictionary<string, object?>? ParseArguments(string? argumentsJson)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson)) return null;

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(argumentsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
