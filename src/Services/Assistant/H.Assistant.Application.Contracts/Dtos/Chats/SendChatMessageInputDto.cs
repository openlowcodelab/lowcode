namespace H.Assistant.Application.Contracts;

/// <summary>
/// 发送聊天消息输入 DTO
/// </summary>
public class SendChatMessageInputDto
{
    public Guid? SessionId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? AgentType { get; set; }

    /// <summary>
    /// 模型配置 ID（指定使用哪个模型配置，为空则使用默认）
    /// </summary>
    public Guid? ModelConfigId { get; set; }

    /// <summary>
    /// 指定使用的 LLM Provider 名称（向后兼容，优先使用 ModelConfigId）
    /// </summary>
    public string? ProviderName { get; set; }
}
