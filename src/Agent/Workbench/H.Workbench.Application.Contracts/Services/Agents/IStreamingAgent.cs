namespace H.Workbench.Application.Contracts;

/// <summary>
/// 流式 Agent 接口，支持逐块返回响应
/// </summary>
public interface IStreamingAgent : IAgentInstance
{
    /// <summary>
    /// 流式处理消息，逐块返回响应内容；ct 用于客户端断连时终止执行（防审批悬挂与无效算力消耗）
    /// </summary>
    IAsyncEnumerable<string> ProcessMessageStreamAsync(string message, List<string>? conversationHistory = null, CancellationToken ct = default);
}
