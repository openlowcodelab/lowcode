namespace H.Workbench.Application.Contracts;

/// <summary>
/// 运行看板聚合结果：回答"这些数字员工干得怎么样、花了多少"。
/// 只有轨迹没有这套账时，员工就只是个聊天窗口。
/// </summary>
public class RuntimeStatsDto
{
    public int Days { get; set; }
    public DateTime From { get; set; }

    public int TotalRuns { get; set; }
    public int SuccessRuns { get; set; }
    public int FailedRuns { get; set; }
    public int OtherRuns { get; set; }

    /// <summary>Success / (Success+Failed)，其余状态（Running/Cancelled/Abandoned）不计入分母</summary>
    public double SuccessRate { get; set; }

    public int VerdictPass { get; set; }
    public int VerdictFail { get; set; }
    public int VerdictUnclear { get; set; }

    /// <summary>未设验收标准的执行数——这些"完成"没有独立判据</summary>
    public int UnverifiedRuns { get; set; }

    public long PromptTokens { get; set; }
    public long CompletionTokens { get; set; }
    public double AvgDurationSeconds { get; set; }

    public int PendingApprovals { get; set; }
    public int PendingArtifactReviews { get; set; }

    public List<AgentRuntimeStat> ByAgent { get; set; } = [];
    public List<TaskRuntimeStat> TopTasks { get; set; } = [];
}

public class AgentRuntimeStat
{
    public string AgentType { get; set; } = string.Empty;
    public int Runs { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public int VerdictFail { get; set; }
    public long Tokens { get; set; }
    public double AvgDurationSeconds { get; set; }
}

public class TaskRuntimeStat
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? AgentType { get; set; }
    public int Runs { get; set; }
    public int Failed { get; set; }
    public long Tokens { get; set; }
}
