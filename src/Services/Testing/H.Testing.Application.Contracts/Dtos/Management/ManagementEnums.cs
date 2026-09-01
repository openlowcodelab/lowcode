namespace H.Testing.Application.Contracts;

/// <summary>
/// 需求优先级
/// </summary>
public enum RequirementPriority
{
    P0 = 1,
    P1 = 2,
    P2 = 3,
    P3 = 4
}

/// <summary>
/// 需求状态
/// </summary>
public enum RequirementStatus
{
    Active = 1,
    Closed = 2
}

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
