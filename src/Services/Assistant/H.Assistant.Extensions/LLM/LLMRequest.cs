using System.Text.Json.Serialization;

namespace H.Assistant.Extensions;

/// <summary>
/// 对话消息
/// </summary>
public class Message
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;  // system/user/assistant
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
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
