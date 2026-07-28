using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 聊天消息 DTO
/// </summary>
public class ChatMessageDto : EntityDto<Guid>
{
    public Guid SessionId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public string? ToolName { get; set; }

    public string? ToolResult { get; set; }
}
