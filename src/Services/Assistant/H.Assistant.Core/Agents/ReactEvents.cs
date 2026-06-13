namespace H.Assistant.Core.Agents;

/// <summary>
/// ReAct 事件基类
/// </summary>
public abstract class ReactEvent
{
    public string Type { get; protected set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Iteration { get; set; }
}

/// <summary>
/// LLM 思考事件（流式增量）
/// </summary>
public class ThinkingEvent : ReactEvent
{
    public ThinkingEvent() { Type = "thinking"; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 工具调用事件
/// </summary>
public class ToolCallingEvent : ReactEvent
{
    public ToolCallingEvent() { Type = "tool_call"; }
    public string ToolName { get; set; } = string.Empty;
    public string ToolCallId { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}

/// <summary>
/// 工具执行结果事件
/// </summary>
public class ToolResultEvent : ReactEvent
{
    public ToolResultEvent() { Type = "tool_result"; }
    public string ToolName { get; set; } = string.Empty;
    public string ToolCallId { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public bool IsError { get; set; }
}

/// <summary>
/// 最终回答事件（流式增量）
/// </summary>
public class FinalAnswerEvent : ReactEvent
{
    public FinalAnswerEvent() { Type = "answer"; }
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 错误事件
/// </summary>
public class ErrorEvent : ReactEvent
{
    public ErrorEvent() { Type = "error"; }
    public string Message { get; set; } = string.Empty;
    public bool IsFatal { get; set; }
}
