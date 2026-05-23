namespace H.Agent.Application.Contracts;

/// <summary>
/// Agent 实例接口
/// </summary>
public interface IAgentInstance
{
    string Name { get; }
    string SystemPrompt { get; }
    Task<string> ProcessMessageAsync(string message, List<string>? conversationHistory = null);
    List<string> GetAvailableTools();
}
