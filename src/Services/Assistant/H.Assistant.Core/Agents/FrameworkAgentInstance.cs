using H.Assistant.Application.Contracts;
using Microsoft.Agents.AI;

namespace H.Assistant.Core;

/// <summary>
/// 基于 Microsoft Agent Framework 的 Agent 实例包装器
/// </summary>
public class FrameworkAgentInstance : IAgentInstance, IStreamingAgent
{
    private readonly FrameworkAgent _frameworkAgent;
    private readonly AgentDefinitionDto _definition;
    private readonly List<SkillDefinitionDto> _skills;

    public FrameworkAgentInstance(
        ILLMProvider llmProvider,
        AgentDefinitionDto definition,
        List<SkillDefinitionDto> skills)
    {
        _definition = definition;
        _skills = skills;
        _frameworkAgent = new FrameworkAgent(llmProvider, definition, skills);
    }

    public string Name => _definition.DisplayName;
    public string SystemPrompt => _definition.SystemPrompt;

    public async Task<string> ProcessMessageAsync(string message, List<string>? conversationHistory = null)
    {
        // 如果有会话历史，使用会话管理
        if (conversationHistory != null && conversationHistory.Any())
        {
            var session = await _frameworkAgent.CreateSessionAsync();
            var fullMessage = string.Join("\n", conversationHistory) + "\nuser: " + message;
            return await _frameworkAgent.RunWithSessionAsync(message, session);
        }

        return await _frameworkAgent.RunAsync(message);
    }

    public async IAsyncEnumerable<string> ProcessMessageStreamAsync(string message, List<string>? conversationHistory = null)
    {
        if (_definition.SupportsStreaming)
        {
            AgentSession? session = null;
            if (conversationHistory != null && conversationHistory.Any())
            {
                session = await _frameworkAgent.CreateSessionAsync();
            }

            await foreach (var chunk in _frameworkAgent.RunStreamingWithSessionAsync(message, session))
            {
                yield return chunk;
            }
        }
        else
        {
            // 降级到非流式
            var response = await ProcessMessageAsync(message, conversationHistory);
            yield return response;
        }
    }

    public List<string> GetAvailableTools()
    {
        return _skills
            .Where(s => s.IsEnabled)
            .Select(s => s.DisplayName)
            .ToList();
    }
}
