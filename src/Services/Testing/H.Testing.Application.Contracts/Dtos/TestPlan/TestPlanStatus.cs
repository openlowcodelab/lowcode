namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试计划状态
/// </summary>
public enum TestPlanStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    Archived = 4
}

/// <summary>
/// 测试计划执行方式
/// </summary>
public enum TestPlanTriggerType
{
    /// <summary>手动执行（默认）</summary>
    Manual = 0,

    /// <summary>定时执行（按 Cron 表达式自动触发）</summary>
    Scheduled = 1
}

/// <summary>
/// 计划内用例状态
/// </summary>
public enum PlanCaseStatus
{
    NotStarted = 0,
    Passed = 1,
    Failed = 2,
    Blocked = 3
}

/// <summary>
/// 缺陷严重程度
/// </summary>
public enum DefectSeverity
{
    Critical = 1,
    Major = 2,
    Minor = 3,
    Trivial = 4
}

/// <summary>
/// 缺陷状态
/// </summary>
public enum DefectStatus
{
    Open = 1,
    Resolved = 2,
    Closed = 3
}
