using System.Text.Json.Serialization;

namespace H.Assistant.Core;

/// <summary>
/// 对话消息
/// </summary>
public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;  // system/user/assistant/tool
    
    [JsonPropertyName("content")]
    public string? Content { get; set; }
    
    /// <summary>
    /// assistant 消息中的 tool_calls（OpenAI 协议）
    /// </summary>
    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ToolCall>? ToolCalls { get; set; }
    
    /// <summary>
    /// tool 消息中的 tool_call_id（OpenAI 协议）
    /// </summary>
    [JsonPropertyName("tool_call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolCallId { get; set; }
}

/// <summary>
/// 工具定义
/// </summary>
public class ToolDefinition
{
    public string Type { get; set; } = "function";
    public FunctionDefinition Function { get; set; } = new();
}

public class FunctionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Parameters { get; set; }
}

/// <summary>
/// LLM 请求
/// </summary>
public class LLMRequest
{
    public string Model { get; set; } = string.Empty;
    public List<Message> Messages { get; set; } = new();
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 2000;
    public List<ToolDefinition>? Tools { get; set; }
}
