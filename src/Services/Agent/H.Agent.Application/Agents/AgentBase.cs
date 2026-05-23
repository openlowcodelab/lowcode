using H.Agent.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace H.Agent.Application;

/// <summary>
/// Agent 基类 - 提供 LLM 调用能力，支持同步和流式响应
/// </summary>
public abstract class AgentBase : IAgentInstance, IStreamingAgent
{
    protected readonly ILLMProvider? _llmProvider;

    protected AgentBase(ILLMProvider? llmProvider)
    {
        _llmProvider = llmProvider;
    }

    public abstract string Name { get; }
    public abstract string SystemPrompt { get; }
    public abstract List<string> GetAvailableTools();

    public virtual async Task<string> ProcessMessageAsync(string message, List<string>? conversationHistory = null)
    {
        if (_llmProvider == null)
        {
            return "[系统] 未配置 LLM Provider，请在模型配置页面添加并启用 Provider。";
        }

        try
        {
            var messages = BuildMessages(message, conversationHistory);
            var request = new LLMRequest
            {
                Messages = messages,
                Temperature = 0.7f,
                MaxTokens = 2000
            };

            var response = await _llmProvider.ChatAsync(request);
            return response.Content;
        }
        catch (Exception ex)
        {
            return $"[错误] 调用 LLM 失败：{ex.Message}";
        }
    }

    public virtual async IAsyncEnumerable<string> ProcessMessageStreamAsync(string message, List<string>? conversationHistory = null)
    {
        if (_llmProvider == null)
        {
            yield return "[系统] 未配置 LLM Provider，请在模型配置页面添加并启用 Provider。";
            yield break;
        }

        var messages = BuildMessages(message, conversationHistory);
        var request = new LLMRequest
        {
            Messages = messages,
            Temperature = 0.7f,
            MaxTokens = 2000
        };

        // 使用流式 API
        await foreach (var chunk in _llmProvider.ChatStreamAsync(request))
        {
            yield return chunk;
        }
    }

    /// <summary>
    /// 构建消息列表
    /// </summary>
    protected List<Message> BuildMessages(string message, List<string>? conversationHistory)
    {
        var messages = new List<Message>
        {
            new Message { Role = "system", Content = SystemPrompt }
        };

        if (conversationHistory != null)
        {
            foreach (var hist in conversationHistory)
            {
                var parts = hist.Split(':', 2);
                if (parts.Length == 2)
                {
                    messages.Add(new Message
                    {
                        Role = parts[0].Trim().ToLower() == "user" ? "user" : "assistant",
                        Content = parts[1].Trim()
                    });
                }
            }
        }

        messages.Add(new Message { Role = "user", Content = message });
        return messages;
    }
}
