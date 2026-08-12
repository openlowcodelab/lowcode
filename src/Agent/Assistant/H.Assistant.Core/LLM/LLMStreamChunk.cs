namespace H.Assistant.Core;

/// <summary>
/// LLM 流式响应 chunk
/// </summary>
public class LLMStreamChunk
{
    /// <summary>
    /// 文本增量
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 工具调用增量
    /// </summary>
    public ToolCallDelta? ToolCallDelta { get; set; }

    /// <summary>
    /// 完成原因: "stop" | "tool_calls"
    /// </summary>
    public string? FinishReason { get; set; }
}

/// <summary>
/// 工具调用流式增量
/// </summary>
public class ToolCallDelta
{
    public int Index { get; set; }
    public string? Id { get; set; }
    public string? FunctionName { get; set; }
    public string? FunctionArgumentsDelta { get; set; }
}
