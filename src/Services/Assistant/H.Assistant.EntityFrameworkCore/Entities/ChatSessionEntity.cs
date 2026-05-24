using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// 聊天会话实体
/// </summary>
public class ChatSessionEntity : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// 会话标题
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// 最后消息时间
    /// </summary>
    public DateTime LastMessageTime { get; set; }
    
    /// <summary>
    /// 消息数量
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// Agent 类型
    /// </summary>
    public string AgentType { get; set; } = string.Empty;
    
    /// <summary>
    /// 导航属性：该会话下的消息
    /// </summary>
    public List<ChatMessageEntity> Messages { get; set; } = new();
}
