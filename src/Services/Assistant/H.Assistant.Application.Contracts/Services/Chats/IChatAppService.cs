using H.Abstractions;

namespace H.Assistant.Application.Contracts;

public interface IChatAppService : IAppService
{
    /// <summary>
    /// 创建新会话
    /// </summary>
    Task<Guid> CreateSessionAsync(string title, string agentType = "general");
    
    /// <summary>
    /// 获取单个会话
    /// </summary>
    Task<ChatDto?> GetSessionAsync(Guid sessionId);
    
    /// <summary>
    /// 获取会话列表
    /// </summary>
    Task<List<ChatDto>> GetSessionsAsync(string? filter = null);
    
    /// <summary>
    /// 添加消息到会话
    /// </summary>
    Task AddMessageAsync(Guid sessionId, ChatMessageDto message);
    
    /// <summary>
    /// 获取会话消息历史
    /// </summary>
    Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId);
    
    /// <summary>
    /// 删除会话及其其消息
    /// </summary>
    Task DeleteSessionAsync(Guid sessionId);
}
