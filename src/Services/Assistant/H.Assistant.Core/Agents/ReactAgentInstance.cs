using System.Text.Json;
using System.Text.Encodings.Web;
using H.Assistant.Application.Contracts;
using H.Assistant.Core.Agents;
using Microsoft.Extensions.Logging;

namespace H.Assistant.Core;

/// <summary>
/// ReAct Agent 实例包装器 - 实现 IAgentInstance + IStreamingAgent
/// </summary>
public class ReactAgentInstance : IAgentInstance, IStreamingAgent
{
    private readonly ILLMProvider _llmProvider;
    private readonly AgentDto _definition;
    private readonly ToolExecutor _toolExecutor;
    private readonly List<ToolDefinition> _toolDefs;
    private readonly ILogger<ReactAgent> _reactLogger;
    private readonly ILogger<ReactAgentInstance> _logger;

    public ReactAgentInstance(
        ILLMProvider llmProvider,
        AgentDto definition,
        ToolExecutor toolExecutor,
        List<ToolDefinition> toolDefs,
        ILogger<ReactAgent> reactLogger,
        ILogger<ReactAgentInstance> logger)
    {
        _llmProvider = llmProvider;
        _definition = definition;
        _toolExecutor = toolExecutor;
        _toolDefs = toolDefs;
        _reactLogger = reactLogger;
        _logger = logger;
    }

    public string Name => _definition.DisplayName;
    public string SystemPrompt => _definition.SystemPrompt;

    /// <summary>
    /// 非流式处理：收集所有事件，返回最终答案
    /// </summary>
    public async Task<string> ProcessMessageAsync(string message, List<string>? conversationHistory = null)
    {
        var history = BuildHistory(conversationHistory);
        var agent = new ReactAgent(_llmProvider, _toolExecutor, _toolDefs, _reactLogger);

        var finalAnswer = string.Empty;

        await foreach (var evt in agent.RunAsync(message, history, SystemPrompt, GetMaxIterations()))
        {
            if (evt is ThinkingEvent thinking)
            {
                // 思考内容累积为最终回答（因为 FinalAnswerEvent 现在为空）
                finalAnswer += thinking.Content;
            }
            else if (evt is FinalAnswerEvent answer)
            {
                // 如果 FinalAnswerEvent 有内容则使用，否则 finalAnswer 已从 thinking 累积
                if (!string.IsNullOrEmpty(answer.Content))
                {
                    finalAnswer = answer.Content;
                }
            }
            else if (evt is ErrorEvent error && error.IsFatal)
            {
                if (string.IsNullOrEmpty(finalAnswer))
                    finalAnswer = $"执行出错: {error.Message}";
            }
        }

        return string.IsNullOrEmpty(finalAnswer) ? "(Agent 未返回结果)" : finalAnswer;
    }

    /// <summary>
    /// 流式处理：将 ReactEvent 序列化为 JSON 字符串逐块返回
    /// </summary>
    public async IAsyncEnumerable<string> ProcessMessageStreamAsync(string message, List<string>? conversationHistory = null)
    {
        var history = BuildHistory(conversationHistory);
        var agent = new ReactAgent(_llmProvider, _toolExecutor, _toolDefs, _reactLogger);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        await foreach (var evt in agent.RunAsync(message, history, SystemPrompt, GetMaxIterations()))
        {
            // 按事件类型序列化，只包含相关字段（避免 null 污染）
            object payload = evt switch
            {
                ThinkingEvent t => new { type = t.Type, content = t.Content, iteration = t.Iteration },
                ToolCallingEvent tc => new { type = tc.Type, toolName = tc.ToolName, toolCallId = tc.ToolCallId, arguments = tc.Arguments, iteration = tc.Iteration },
                ToolResultEvent tr => new { type = tr.Type, toolName = tr.ToolName, toolCallId = tr.ToolCallId, result = tr.Result, isError = tr.IsError, iteration = tr.Iteration },
                FinalAnswerEvent a => new { type = a.Type, content = a.Content, iteration = a.Iteration },
                ErrorEvent e => new { type = e.Type, message = e.Message, isFatal = e.IsFatal, iteration = e.Iteration },
                _ => new { type = evt.Type, iteration = evt.Iteration }
            };

            yield return JsonSerializer.Serialize(payload, jsonOptions);
        }
    }

    public List<string> GetAvailableTools()
    {
        return _toolDefs.Select(t => t.Function.Name).ToList();
    }

    /// <summary>
    /// 从 conversationHistory 字符串列表构建 Message 列表
    /// </summary>
    private static List<Message> BuildHistory(List<string>? conversationHistory)
    {
        var history = new List<Message>();
        if (conversationHistory == null || conversationHistory.Count == 0) return history;

        foreach (var line in conversationHistory)
        {
            var colonIdx = line.IndexOf(':');
            if (colonIdx > 0)
            {
                var role = line[..colonIdx].Trim().ToLowerInvariant();
                var content = line[(colonIdx + 1)..].Trim();
                if (role is "user" or "assistant" or "system" or "tool")
                {
                    history.Add(new Message { Role = role, Content = content });
                }
            }
        }

        return history;
    }

    /// <summary>
    /// 从 Agent 元数据获取最大迭代次数
    /// </summary>
    private int GetMaxIterations()
    {
        if (!string.IsNullOrWhiteSpace(_definition.Metadata))
        {
            try
            {
                var meta = JsonSerializer.Deserialize<JsonElement>(_definition.Metadata);
                if (meta.TryGetProperty("maxIterations", out var maxIter))
                {
                    return maxIter.GetInt32();
                }
            }
            catch { }
        }
        return 10; // 默认
    }
}
