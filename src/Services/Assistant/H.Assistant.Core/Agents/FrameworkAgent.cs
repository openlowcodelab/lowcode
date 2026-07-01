using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using H.Assistant.Application.Contracts;
using System.Text.Json;

namespace H.Assistant.Core;

/// <summary>
/// 基于 Microsoft Agent Framework 的 Agent 实现
/// 使用 AIAgent、工具系统和会话管理
/// </summary>
public class FrameworkAgent
{
    private readonly AIAgent _agent;
    private readonly ILLMProvider _llmProvider;
    private readonly AgentDto _definition;
    private readonly List<SkillDto> _skills;

    public FrameworkAgent(
        ILLMProvider llmProvider,
        AgentDto definition,
        List<SkillDto> skills)
    {
        _llmProvider = llmProvider;
        _definition = definition;
        _skills = skills;

        // 创建工具列表
        var tools = BuildTools();

        // 使用 Microsoft Agent Framework 创建 Agent
        _agent = llmProvider.AsAIAgent(
            name: definition.DisplayName,
            instructions: definition.SystemPrompt,
            temperature: definition.Temperature,
            maxTokens: definition.MaxTokens,
            tools: tools);
    }

    /// <summary>
    /// 运行单次对话
    /// </summary>
    public async Task<string> RunAsync(string message, CancellationToken ct = default)
    {
        var result = await _agent.RunAsync(message, cancellationToken: ct);
        return result.ToString();
    }

    /// <summary>
    /// 运行带会话的多轮对话
    /// </summary>
    public async Task<string> RunWithSessionAsync(
        string message,
        AgentSession? session = null,
        CancellationToken ct = default)
    {
        var result = await _agent.RunAsync(message, session, cancellationToken: ct);
        return result.ToString();
    }

    /// <summary>
    /// 流式运行带会话的多轮对话
    /// </summary>
    public async IAsyncEnumerable<string> RunStreamingWithSessionAsync(
        string message,
        AgentSession? session = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var update in _agent.RunStreamingAsync(message, session, cancellationToken: ct))
        {
            yield return update.ToString();
        }
    }

    /// <summary>
    /// 创建新会话
    /// </summary>
    public async Task<AgentSession> CreateSessionAsync(CancellationToken ct = default)
    {
        return await _agent.CreateSessionAsync(cancellationToken: ct);
    }

    /// <summary>
    /// 序列化会话状态
    /// </summary>
    public async Task<JsonElement> SerializeSessionAsync(AgentSession session, CancellationToken ct = default)
    {
        return await _agent.SerializeSessionAsync(session, cancellationToken: ct);
    }

    /// <summary>
    /// 反序列化会话状态
    /// </summary>
    public async Task<AgentSession> DeserializeSessionAsync(
        JsonElement sessionState,
        CancellationToken ct = default)
    {
        return await _agent.DeserializeSessionAsync(sessionState, cancellationToken: ct);
    }

    /// <summary>
    /// 构建工具列表
    /// </summary>
    private List<AIFunction> BuildTools()
    {
        var tools = new List<AIFunction>();

        foreach (var skill in _skills.Where(s => s.IsEnabled))
        {
            var tool = CreateToolFromSkill(skill);
            if (tool != null)
            {
                tools.Add(tool);
            }
        }

        return tools;
    }

    /// <summary>
    /// 从技能定义创建工具
    /// </summary>
    private AIFunction? CreateToolFromSkill(SkillDto skill)
    {
        if (string.IsNullOrWhiteSpace(skill.ImplementationClass))
        {
            return null;
        }

        try
        {
            // 尝试从程序集加载技能实现
            var type = Type.GetType(skill.ImplementationClass);
            if (type == null)
            {
                return null;
            }

            // 使用 AIFunctionFactory 创建工具
            var method = type.GetMethod("Execute");
            if (method == null)
            {
                return null;
            }

            var instance = Activator.CreateInstance(type);

            // 使用 Description 属性帮助 AI 理解工具
            var tool = AIFunctionFactory.Create(
                method,
                instance,
                new AIFunctionFactoryOptions
                {
                    Name = skill.SkillName,
                    Description = skill.Description
                });

            return tool;
        }
        catch
        {
            return null;
        }
    }
}
