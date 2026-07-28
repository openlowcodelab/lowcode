namespace H.Assistant.Core;

/// <summary>
/// 工具调用
/// </summary>
public class ToolCall
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "function";
    public FunctionCall Function { get; set; } = new();
}

public class FunctionCall
{
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// LLM 响应
/// </summary>
public class LLMResponse
{
    public string Content { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int UsageTokens { get; set; }
    public List<ToolCall>? ToolCalls { get; set; }
}
