namespace H.Workbench.Application.Contracts;

/// <summary>
/// 工作流步骤定义（任务创建方式为"工作流"时使用，序列化存储于 WorkflowContent）。
/// 旧数据只有 Name/Prompt，新字段全部可空/有默认值，反序列化向后兼容。
/// </summary>
public class WorkflowStepDto
{
    /// <summary>步骤稳定标识（编辑器生成，短 Guid）</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>步骤名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>步骤提示词/指令</summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>步骤指派员工（多员工接力）；空=任务默认员工</summary>
    public string? AgentType { get; set; }

    /// <summary>人工卡点：交互执行时该步骤前暂停等待批准（复用工具审批通道）；定时执行自动通过</summary>
    public bool RequireApproval { get; set; }

    /// <summary>失败策略：Stop（默认，中止工作流）| Skip（记录失败继续下一步）</summary>
    public string OnFailure { get; set; } = "Stop";

    /// <summary>将本步结果注册为命名变量，供后续步骤引用；留空自动用步骤名</summary>
    public string? OutputVar { get; set; }
}

/// <summary>
/// 工作流执行结果汇总（序列化存储于 TaskLog.Result）
/// </summary>
public class WorkflowRunSummaryDto
{
    public bool Success { get; set; }
    public int CompletedSteps { get; set; }
    public int FailedSteps { get; set; }
    public int SkippedSteps { get; set; }
    /// <summary>失败/中止原因（人工卡点被拒、步骤异常等）</summary>
    public string? Error { get; set; }
    public List<WorkflowStepResultDto> Steps { get; set; } = new();
    /// <summary>最后一步的完整输出（下游消费主体）</summary>
    public string? FinalOutput { get; set; }
}

public class WorkflowStepResultDto
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AgentType { get; set; }
    /// <summary>Success | Failed | Skipped（OnFailure=Skip 或卡点拒绝）</summary>
    public string State { get; set; } = string.Empty;
    /// <summary>结果前 400 字预览（全文走 trace API）</summary>
    public string? Preview { get; set; }
    public string? Error { get; set; }
}
