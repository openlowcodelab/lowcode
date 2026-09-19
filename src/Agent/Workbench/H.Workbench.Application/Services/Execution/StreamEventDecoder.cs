using System.Text.Json;

namespace H.Workbench.Application.Services.Execution;

public enum StreamEventKind
{
    Unknown,
    Thinking,
    ToolCall,
    ToolResult,
    ApprovalRequired,
    ApprovalResolved,
    Answer,
    Error
}

/// <summary>
/// SSE/回调 JSON 负载的解码结果（字段按事件类型取用，未携带的为默认值）
/// </summary>
public sealed class DecodedEvent
{
    public StreamEventKind Kind { get; init; }
    public int Iteration { get; init; }
    public string? Content { get; init; }
    public string? ToolName { get; init; }
    public string? SkillName { get; init; }
    public string? ToolCallId { get; init; }
    public string? Arguments { get; init; }
    public string? Result { get; init; }
    public bool IsError { get; init; }
    public bool Truncated { get; init; }
    public int? DurationMs { get; init; }
    public Guid ApprovalId { get; init; }
    public string? Decision { get; init; }
    public long? WaitMs { get; init; }
    public string? Message { get; init; }
    public bool IsFatal { get; init; } = true;
}

/// <summary>
/// ReactAgentInstance 事件 JSON 负载解码器。
/// 负载形状只在 Core 的 SerializeEvent 一处生成，此处全部 TryGetProperty、未知 type 返回 null——
/// 契约脆性不会让轨迹记录抛异常影响主执行。
/// </summary>
public static class StreamEventDecoder
{
    public static DecodedEvent? Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith('{'))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("type", out var typeProp))
            {
                return null;
            }

            var type = typeProp.GetString();
            var iteration = GetInt(root, "iteration");

            return type switch
            {
                "thinking" => new DecodedEvent
                {
                    Kind = StreamEventKind.Thinking,
                    Iteration = iteration,
                    Content = GetString(root, "content")
                },
                "tool_call" => new DecodedEvent
                {
                    Kind = StreamEventKind.ToolCall,
                    Iteration = iteration,
                    ToolName = GetString(root, "toolName"),
                    SkillName = GetString(root, "skillName"),
                    ToolCallId = GetString(root, "toolCallId"),
                    Arguments = GetString(root, "arguments")
                },
                "tool_result" => new DecodedEvent
                {
                    Kind = StreamEventKind.ToolResult,
                    Iteration = iteration,
                    ToolName = GetString(root, "toolName"),
                    SkillName = GetString(root, "skillName"),
                    ToolCallId = GetString(root, "toolCallId"),
                    Result = GetString(root, "result"),
                    IsError = GetBool(root, "isError"),
                    Truncated = GetBool(root, "truncated"),
                    DurationMs = root.TryGetProperty("durationMs", out var d) && d.TryGetInt32(out var dv) ? dv : null
                },
                "approval_required" => new DecodedEvent
                {
                    Kind = StreamEventKind.ApprovalRequired,
                    Iteration = iteration,
                    ApprovalId = GetGuid(root, "approvalId"),
                    ToolName = GetString(root, "toolName"),
                    SkillName = GetString(root, "skillName"),
                    ToolCallId = GetString(root, "toolCallId"),
                    Arguments = GetString(root, "arguments")
                },
                "approval_resolved" => new DecodedEvent
                {
                    Kind = StreamEventKind.ApprovalResolved,
                    Iteration = iteration,
                    ApprovalId = GetGuid(root, "approvalId"),
                    ToolName = GetString(root, "toolName"),
                    Decision = GetString(root, "decision"),
                    WaitMs = root.TryGetProperty("waitMs", out var w) && w.TryGetInt64(out var wv) ? wv : null
                },
                "answer" => new DecodedEvent
                {
                    Kind = StreamEventKind.Answer,
                    Iteration = iteration,
                    Content = GetString(root, "content")
                },
                "error" => new DecodedEvent
                {
                    Kind = StreamEventKind.Error,
                    Iteration = iteration,
                    Message = GetString(root, "message"),
                    IsFatal = !root.TryGetProperty("isFatal", out var f) || f.GetBoolean()
                },
                _ => null
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? GetString(JsonElement root, string name) =>
        root.TryGetProperty(name, out var v) ? v.GetString() : null;

    private static bool GetBool(JsonElement root, string name) =>
        root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;

    private static int GetInt(JsonElement root, string name) =>
        root.TryGetProperty(name, out var v) && v.TryGetInt32(out var i) ? i : 0;

    private static Guid GetGuid(JsonElement root, string name) =>
        root.TryGetProperty(name, out var v) && v.TryGetGuid(out var g) ? g : Guid.Empty;
}
