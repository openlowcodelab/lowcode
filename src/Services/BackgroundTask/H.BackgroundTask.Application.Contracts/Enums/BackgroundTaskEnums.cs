namespace H.BackgroundTask.Application.Contracts;

/// <summary>
/// 任务调度方式
/// </summary>
public enum JobTriggerKind
{
    /// <summary>一次性任务（在指定时间执行一次，未指定时间则立即执行）</summary>
    OneTime = 0,

    /// <summary>周期任务（按 Cron 表达式周期执行）</summary>
    Recurring = 1
}

/// <summary>
/// 任务执行类型
/// </summary>
public enum JobExecuteType
{
    /// <summary>调用 HTTP API</summary>
    Api = 0,

    /// <summary>执行 SQL 语句</summary>
    Sql = 1
}

/// <summary>
/// 任务执行结果状态
/// </summary>
public enum JobExecutionStatus
{
    /// <summary>执行成功</summary>
    Success = 0,

    /// <summary>执行失败</summary>
    Failed = 1
}
