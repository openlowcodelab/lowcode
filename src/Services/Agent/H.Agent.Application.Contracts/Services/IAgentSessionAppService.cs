using Volo.Abp.Application.Services;

namespace H.Agent.Application.Contracts;

/// <summary>
/// Agent 会话管理服务接口
/// </summary>
public interface IAgentSessionAppService : IApplicationService
{
    /// <summary>
    /// 创建新会话
    /// </summary>
    Task<Guid> CreateSessionAsync(string title, string agentType = "general");
    
    /// <summary>
    /// 获取单个会话
    /// </summary>
    Task<ChatSessionDto?> GetSessionAsync(Guid sessionId);
    
    /// <summary>
    /// 获取会话列表
    /// </summary>
    Task<List<ChatSessionDto>> GetSessionsAsync(string? filter = null);
    
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
