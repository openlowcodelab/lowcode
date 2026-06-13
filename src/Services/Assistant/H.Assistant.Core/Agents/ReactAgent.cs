using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace H.Assistant.Core.Agents;

/// <summary>
/// ReAct Agent 核心循环 - Reasoning + Acting
/// 持续的 思考→行动→观察 直到任务完成
/// </summary>
public class ReactAgent
{
    private readonly ILLMProvider _provider;
    private readonly ToolExecutor _toolExecutor;
    private readonly List<ToolDefinition> _toolDefs;
    private readonly ILogger<ReactAgent> _logger;

    /// <summary>
    /// 默认最大迭代次数
    /// </summary>
    private const int DefaultMaxIterations = 10;

    public ReactAgent(
        ILLMProvider provider,
        ToolExecutor toolExecutor,
        List<ToolDefinition> toolDefs,
        ILogger<ReactAgent> logger)
    {
        _provider = provider;
        _toolExecutor = toolExecutor;
        _toolDefs = toolDefs;
        _logger = logger;
    }

    /// <summary>
    /// 运行 ReAct 循环
    /// </summary>
    public async IAsyncEnumerable<ReactEvent> RunAsync(
        string userMessage,
        List<Message> history,
        string systemPrompt,
        int maxIterations = DefaultMaxIterations,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var messages = new List<Message>();

        // 添加 system prompt
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(new Message { Role = "system", Content = systemPrompt });
        }

        // 添加历史消息
        messages.AddRange(history);

        // 添加当前用户消息
        messages.Add(new Message { Role = "user", Content = userMessage });

        _logger.LogInformation("ReAct 循环开始: 总消息数={Count}, 最大迭代={MaxIter}", messages.Count, maxIterations);

        for (int iteration = 1; iteration <= maxIterations; iteration++)
        {
            if (ct.IsCancellationRequested) yield break;

            _logger.LogInformation("=== ReAct 迭代 {Iteration} ===", iteration);

            var request = new LLMRequest
            {
                Messages = messages,
                Tools = _toolDefs.Count > 0 ? _toolDefs : null
            };

            _logger.LogDebug("LLM 请求: 消息数={MsgCount}, 工具数={ToolCount}", 
                messages.Count, _toolDefs?.Count ?? 0);

            // 流式调用 LLM - 使用 Channel 桥接 try-catch 和 yield return
            // (C# 不允许在包含 catch 的 try 块中 yield return)
            var contentBuffer = string.Empty;
            var toolCallsMap = new Dictionary<int, AccumulatedToolCall>();
            var chunkChannel = Channel.CreateUnbounded<LLMStreamChunk>();
            Exception? llmException = null;

            // 后台任务：流式读取 LLM 响应并写入 Channel
            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var chunk in _provider.ChatStreamAsync(request, ct))
                    {
                        await chunkChannel.Writer.WriteAsync(chunk, ct);
                    }
                }
                catch (Exception ex)
                {
                    llmException = ex;
                    _logger.LogError(ex, "LLM 调用失败 (迭代 {Iteration})", iteration);
                }
                finally
                {
                    chunkChannel.Writer.Complete();
                }
            });

            // 从 Channel 读取 chunk 并 yield 思考事件（在 try-catch 外部，可以 yield return）
            await foreach (var chunk in chunkChannel.Reader.ReadAllAsync(ct))
            {
                // 文本增量 → 立即 yield 思考事件（实时推送到前端）
                if (!string.IsNullOrEmpty(chunk.Content))
                {
                    contentBuffer += chunk.Content;
                    _logger.LogDebug("Yield ThinkingEvent: Len={Len}", chunk.Content.Length);
                    yield return new ThinkingEvent
                    {
                        Content = chunk.Content,
                        Iteration = iteration
                    };
                }

                // 工具调用增量 → 累积
                if (chunk.ToolCallDelta != null)
                {
                    var delta = chunk.ToolCallDelta;
                    _logger.LogDebug("收到 ToolCallDelta: Index={Index}, Name={Name}, ArgsDelta={Args}",
                        delta.Index, delta.FunctionName, delta.FunctionArgumentsDelta?.Length ?? 0);

                    if (!toolCallsMap.ContainsKey(delta.Index))
                    {
                        toolCallsMap[delta.Index] = new AccumulatedToolCall
                        {
                            Index = delta.Index,
                            Id = delta.Id ?? string.Empty,
                            FunctionName = delta.FunctionName ?? string.Empty,
                            FunctionArguments = delta.FunctionArgumentsDelta ?? string.Empty
                        };
                    }
                    else
                    {
                        var acc = toolCallsMap[delta.Index];
                        if (delta.Id != null) acc.Id = delta.Id;
                        if (delta.FunctionName != null) acc.FunctionName += delta.FunctionName;
                        if (delta.FunctionArgumentsDelta != null) acc.FunctionArguments += delta.FunctionArgumentsDelta;
                    }
                }
            }

            _logger.LogInformation("LLM 响应完成: ContentLen={Len}, ToolCalls={Count}",
                contentBuffer.Length, toolCallsMap.Count);

            // LLM 调用失败处理
            if (llmException != null)
            {
                yield return new ErrorEvent
                {
                    Message = $"LLM 调用失败: {llmException.Message}",
                    IsFatal = false,
                    Iteration = iteration
                };
                continue;
            }

            // 如果没有 tool_calls → 最终回答（思考内容已实时推送，这里发送完整内容用于持久化）
            if (toolCallsMap.Count == 0)
            {
                _logger.LogInformation("无工具调用，返回最终答案（完成信号）");
                // 发送 FinalAnswerEvent，包含完整的最终回答内容用于数据库持久化
                yield return new FinalAnswerEvent
                {
                    Content = contentBuffer.ToString(),
                    Iteration = iteration
                };
                yield break;
            }

            _logger.LogInformation("发现 {Count} 个工具调用", toolCallsMap.Count);

            // 有 tool_calls → 将 assistant 消息加入 history
            var assistantToolCalls = toolCallsMap.Values
                .OrderBy(tc => tc.Index)
                .Select(tc => new ToolCall
                {
                    Id = tc.Id,
                    Type = "function",
                    Function = new FunctionCall
                    {
                        Name = tc.FunctionName,
                        Arguments = tc.FunctionArguments
                    }
                }).ToList();

            messages.Add(new Message
            {
                Role = "assistant",
                Content = string.IsNullOrEmpty(contentBuffer) ? null : contentBuffer,
                ToolCalls = assistantToolCalls
            });

            // 执行每个工具
            foreach (var toolCall in assistantToolCalls)
            {
                if (ct.IsCancellationRequested) yield break;

                _logger.LogInformation("执行工具: {ToolName}", toolCall.Function.Name);

                yield return new ToolCallingEvent
                {
                    ToolName = toolCall.Function.Name,
                    ToolCallId = toolCall.Id,
                    Arguments = toolCall.Function.Arguments,
                    Iteration = iteration
                };

                var (result, isError) = await _toolExecutor.ExecuteAsync(
                    toolCall.Function.Name,
                    toolCall.Function.Arguments,
                    ct);

                _logger.LogInformation("工具执行完成: {ToolName}, IsError={IsError}, ResultLen={Len}",
                    toolCall.Function.Name, isError, result.Length);

                yield return new ToolResultEvent
                {
                    ToolName = toolCall.Function.Name,
                    ToolCallId = toolCall.Id,
                    Result = result,
                    IsError = isError,
                    Iteration = iteration
                };

                // 将 tool 结果消息加入 history
                messages.Add(new Message
                {
                    Role = "tool",
                    Content = result,
                    ToolCallId = toolCall.Id
                });
            }
        }

        // 超过最大迭代次数
        _logger.LogWarning("达到最大迭代次数 {MaxIter}", maxIterations);
        yield return new ErrorEvent
        {
            Message = $"已达到最大迭代次数 ({maxIterations})，Agent 停止执行",
            IsFatal = true,
            Iteration = maxIterations
        };
    }

    /// <summary>
    /// 流式工具调用累积
    /// </summary>
    private class AccumulatedToolCall
    {
        public int Index { get; set; }
        public string Id { get; set; } = string.Empty;
        public string FunctionName { get; set; } = string.Empty;
        public string FunctionArguments { get; set; } = string.Empty;
    }
}
