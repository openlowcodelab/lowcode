using H.Abp.Application.Contracts;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 任务执行轨迹步骤 DTO（ReAct 单轮内的 thinking/tool/approval/answer/error 一行）
/// </summary>
public class ExecutionStepDto : CreationAuditedEntityDto<Guid>
{
    public Guid TaskLogId { get; set; }
    public Guid TaskId { get; set; }
    public int StepIndex { get; set; }
    public int Iteration { get; set; }
    public int Seq { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string? ToolName { get; set; }
    public string? SkillName { get; set; }
    public string? ToolCallId { get; set; }
    public string? Content { get; set; }
    public string? Arguments { get; set; }
    public string? Result { get; set; }
    public bool IsError { get; set; }
    public string? ApprovalState { get; set; }
    public string? ApproverId { get; set; }
    public int? WaitMs { get; set; }
    public bool Truncated { get; set; }
    public int? DurationMs { get; set; }
    public DateTime StartedAt { get; set; }
}
