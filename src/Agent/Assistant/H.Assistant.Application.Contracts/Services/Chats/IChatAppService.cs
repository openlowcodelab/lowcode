using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

public interface IChatAppService : IAppService
{
    /// <summary>
    /// 创建新会话
    /// </summary>
    Task<BaseOutput<Guid>> CreateSessionAsync(string title, string agentType = "general");

    /// <summary>
    /// 获取单个会话
    /// </summary>
    Task<BaseOutput<ChatDto?>> GetSessionAsync(Guid sessionId);

    /// <summary>
    /// 获取会话列表
    /// </summary>
    Task<BaseOutput<List<ChatDto>>> GetSessionsAsync(string? filter = null);

    /// <summary>
    /// 添加消息到会话
    /// </summary>
    Task<BaseOutput> AddMessageAsync(Guid sessionId, ChatMessageDto message);

    /// <summary>
    /// 获取会话消息历史
    /// </summary>
    Task<BaseOutput<List<ChatMessageDto>>> GetMessagesAsync(Guid sessionId);

    /// <summary>
    /// 删除会话及其其消息
    /// </summary>
    Task<BaseOutput> DeleteSessionAsync(Guid sessionId);
}
