using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace H.Workbench.Core.Agents;

/// <summary>
/// 工具执行结构化结果（轨迹落库与产物派生消费）
/// </summary>
public record ToolExecutionResult(string Text, bool IsError, bool Truncated, int DurationMs);

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

    private readonly int _executionTimeoutSeconds;

    /// <param name="executionTimeoutSeconds">
    /// 兜底超时（秒），须大于各工具内部超时（如 git clone 600s），否则会提前掐断长操作
    /// </param>
    public ToolExecutor(IToolRegistry toolRegistry, ILogger<ToolExecutor> logger, int executionTimeoutSeconds = 660)
    {
        _toolRegistry = toolRegistry;
        _logger = logger;
        _executionTimeoutSeconds = executionTimeoutSeconds > 0 ? executionTimeoutSeconds : 660;
    }

    /// <summary>
    /// 执行工具调用
    /// </summary>
    public async Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        string argumentsJson,
        CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        var tool = _toolRegistry.GetTool(toolName);
        if (tool == null)
        {
            return new ToolExecutionResult(
                $"工具 '{toolName}' 未找到。可用工具: {string.Join(", ", _toolRegistry.GetAllTools().Select(t => t.Name))}",
                true, false, (int)(DateTime.UtcNow - startedAt).TotalMilliseconds);
        }

        try
        {
            var arguments = ParseArguments(argumentsJson);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_executionTimeoutSeconds));

            _logger.LogInformation("执行工具: {ToolName}, 参数: {Args}", toolName,
                argumentsJson.Length > 200 ? argumentsJson[..200] + "..." : argumentsJson);

            var result = await tool.InvokeAsync(
                arguments != null ? new AIFunctionArguments(arguments) : null,
                timeoutCts.Token);

            var resultText = result?.ToString() ?? "(无返回结果)";

            // 截断过长的结果
            var truncated = resultText.Length > MaxResultLength;
            if (truncated)
            {
                resultText = resultText[..MaxResultLength] + "\n...[结果已截断]";
            }

            _logger.LogInformation("工具 {ToolName} 执行成功, 结果长度: {Len}", toolName, resultText.Length);
            return new ToolExecutionResult(resultText, false, truncated, Elapsed(startedAt));
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            var msg = $"工具 '{toolName}' 执行超时（{_executionTimeoutSeconds}秒）";
            _logger.LogWarning(msg);
            return new ToolExecutionResult(msg, true, false, Elapsed(startedAt));
        }
        catch (Exception ex)
        {
            var msg = $"工具 '{toolName}' 执行失败: {ex.Message}";
            _logger.LogWarning(ex, "工具 {ToolName} 执行异常", toolName);
            return new ToolExecutionResult(msg, true, false, Elapsed(startedAt));
        }
    }

    private static int Elapsed(DateTime startedAtUtc) => (int)(DateTime.UtcNow - startedAtUtc).TotalMilliseconds;

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
