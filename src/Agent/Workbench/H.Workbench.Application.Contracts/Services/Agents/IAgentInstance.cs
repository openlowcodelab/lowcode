namespace H.Workbench.Application.Contracts;

/// <summary>
/// Agent 实例接口
/// </summary>
public interface IAgentInstance
{
    string Name { get; }
    string SystemPrompt { get; }

    /// <summary>
    /// 非流式处理；onEventJson 逐事件回调（与流式路径同构的 JSON 负载），供调用方持久化执行轨迹
    /// </summary>
    Task<string> ProcessMessageAsync(string message, List<string>? conversationHistory = null, Func<string, Task>? onEventJson = null);
    List<string> GetAvailableTools();
}
