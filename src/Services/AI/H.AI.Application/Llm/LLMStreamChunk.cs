namespace H.AI.Application;

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

    /// <summary>
    /// token 用量。仅在请求带 stream_options.include_usage 时，由末尾的
    /// usage chunk 携带（该 chunk 的 choices 为空数组）
    /// </summary>
    public LLMUsage? Usage { get; set; }
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
