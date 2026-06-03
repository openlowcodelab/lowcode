using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// Assistant 聊天服务接口
/// </summary>
public interface IChatMessageAppService : IApplicationService
{
    /// <summary>
    /// 发送消息并获取响应
    /// </summary>
    Task<ChatResponseDto> SendMessageAsync(SendChatMessageInputDto input);
    
    /// <summary>
    /// 发送消息并获取流式响应（SSE）
    /// </summary>
    IAsyncEnumerable<string> SendMessageStreamAsync(SendChatMessageInputDto input);
    
    /// <summary>
    /// 获取会话列表
    /// </summary>
    Task<PagedResultDto<ChatDto>> GetSessionsAsync(SessionQueryDto input);
    
    /// <summary>
    /// 获取会话消息历史
    /// </summary>
    Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId);
    
    /// <summary>
    /// 删除会话
    /// </summary>
    Task DeleteSessionAsync(Guid sessionId);
    
    /// <summary>
    /// 获取可用的 Agent 列表
    /// </summary>
    Task<List<AgentConfigDto>> GetAvailableAgentsAsync();
}
