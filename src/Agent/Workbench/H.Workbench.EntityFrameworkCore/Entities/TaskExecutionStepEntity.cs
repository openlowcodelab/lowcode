using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 任务执行轨迹步骤实体：ReAct 每轮的 thinking/tool_call/tool_result/approval/answer/error 逐行落库，
/// 供对话页刷新后与定时任务回放"步骤时间线"
/// </summary>
public class TaskExecutionStepEntity : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// 归属的一次执行（TaskLog）
    /// </summary>
    public Guid TaskLogId { get; set; }

    /// <summary>
    /// 冗余任务ID，供按任务聚合
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 工作流步骤序号（非工作流任务恒为 0；Kind=Step 行为该步头部行）
    /// </summary>
    public int StepIndex { get; set; }

    /// <summary>
    /// ReAct 迭代轮次
    /// </summary>
    public int Iteration { get; set; }

    /// <summary>
    /// 轮内单调递增序号，回放排序键
    /// </summary>
    public int Seq { get; set; }

    /// <summary>
    /// 步骤类型：Thinking/ToolCall/ToolResult/Approval/Answer/Error
    /// </summary>
    public string Kind { get; set; } = string.Empty;

    public string? ToolName { get; set; }

    /// <summary>
    /// 工具归属技能名（来自 ToolRegistry owner）
    /// </summary>
    public string? SkillName { get; set; }

    /// <summary>
    /// 关联 tool_call 与 tool_result / 审批的调用ID
    /// </summary>
    public string? ToolCallId { get; set; }

    /// <summary>
    /// Thinking/Answer 正文（按轮合并后落盘，超长截断）
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 工具入参 JSON（截断）
    /// </summary>
    public string? Arguments { get; set; }

    /// <summary>
    /// 工具结果（ToolExecutor 已截断）
    /// </summary>
    public string? Result { get; set; }

    public bool IsError { get; set; }

    /// <summary>
    /// 审批状态：Pending/Approved/Denied/Timeout/SkippedNonInteractive（仅 Approval 步骤）
    /// </summary>
    public string? ApprovalState { get; set; }

    public string? ApproverId { get; set; }

    /// <summary>
    /// 审批等待时长（毫秒）
    /// </summary>
    public int? WaitMs { get; set; }

    /// <summary>
    /// 文本列是否发生截断
    /// </summary>
    public bool Truncated { get; set; }

    /// <summary>
    /// 工具执行耗时（毫秒）
    /// </summary>
    public int? DurationMs { get; set; }

    public DateTime StartedAt { get; set; }
}
