namespace H.Assistant.Application.Contracts;

/// <summary>
/// 聊天响应 DTO
/// </summary>
public class ChatResponseDto
{
    public Guid SessionId { get; set; }

    public Guid MessageId { get; set; }

    public string Response { get; set; } = string.Empty;

    public bool IsStreaming { get; set; }

    public List<string>? ToolCalls { get; set; }
}
