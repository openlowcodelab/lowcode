using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace H.Agent.Application.Contracts;

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

/// <summary>
/// 聊天会话 DTO
/// </summary>
public class ChatSessionDto : EntityDto<Guid>
{
    public string Title { get; set; } = string.Empty;
    
    public DateTime CreationTime { get; set; }
    
    public DateTime LastMessageTime { get; set; }
    
    public int MessageCount { get; set; }
}

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

/// <summary>
/// Agent 配置 DTO
/// </summary>
public class AgentConfigDto
{
    public string AgentType { get; set; } = string.Empty;
    
    public string DisplayName { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public List<string> Capabilities { get; set; } = new();
}

/// <summary>
/// 会话查询 DTO
/// </summary>
public class SessionQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}
