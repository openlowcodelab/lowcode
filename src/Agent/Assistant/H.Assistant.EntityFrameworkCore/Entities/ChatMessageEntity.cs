using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// 聊天消息实体
/// </summary>
public class ChatMessageEntity : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// 会话 ID
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// 角色（system/user/assistant）
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 工具名称（可选）
    /// </summary>
    public string? ToolName { get; set; }

    /// <summary>
    /// 工具结果（可选）
    /// </summary>
    public string? ToolResult { get; set; }
}
