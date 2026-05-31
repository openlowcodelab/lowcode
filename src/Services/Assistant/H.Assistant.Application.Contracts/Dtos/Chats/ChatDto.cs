using Volo.Abp.Application.Dtos;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 聊天会话 DTO
/// </summary>
public class ChatDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public DateTime LastMessageTime { get; set; }

    public int MessageCount { get; set; }
}
