using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace H.Assistant.Core;

/// <summary>
/// ILLMProvider 到 Microsoft Agent Framework 的扩展方法
/// </summary>
public static class LLMProviderExtensions
{
    /// <summary>
    /// 将 ILLMProvider 转换为 AIAgent
    /// </summary>
    public static AIAgent AsAIAgent(
        this ILLMProvider provider,
        string name,
        string instructions,
        float temperature = 0.7f,
        int maxTokens = 2000,
        IEnumerable<AIFunction>? tools = null)
    {
        // 创建 IChatClient 包装器
        var chatClient = new LLMProviderChatClient(provider);

        // 使用 Microsoft Agent Framework 创建 Agent
        var agentOptions = new ChatClientAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Instructions = instructions,
                Temperature = temperature,
                MaxOutputTokens = maxTokens
            }
        };

        if (tools != null && tools.Any())
        {
            agentOptions.ChatOptions.Tools = tools.Cast<AITool>().ToList();
        }

        return chatClient.AsAIAgent(agentOptions);
    }
}
