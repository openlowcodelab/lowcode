namespace H.Workbench.Application.Contracts;

/// <summary>
/// 审批中心条目：跨任务聚合的工具审批裁决行。
/// Live=false 表示所属执行已经结束（进程内 waiter 已不存在），此时只能查看、无法再回传裁决。
/// </summary>
public class ApprovalQueueItemDto
{
    public Guid Id { get; set; }
    public Guid? ApprovalId { get; set; }
    public Guid TaskLogId { get; set; }
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? AgentType { get; set; }
    public string? ToolName { get; set; }
    public string? SkillName { get; set; }
    public string? Arguments { get; set; }

    /// <summary>Pending/Approved/Denied/Timeout/SkippedNonInteractive/Orphaned</summary>
    public string ApprovalState { get; set; } = string.Empty;

    public string LogStatus { get; set; } = string.Empty;
    public bool Live { get; set; }
    public DateTime StartedAt { get; set; }
    public string? ApproverId { get; set; }
    public int? WaitMs { get; set; }
}
