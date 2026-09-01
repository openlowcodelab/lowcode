namespace H.Testing.Application.Contracts;

/// <summary>
/// 定时计划执行器。由 Hangfire 作业服务器反射调用，需保证方法签名稳定。
/// </summary>
public interface ITestScheduleJobExecutor
{
    /// <summary>
    /// 执行指定的定时计划
    /// </summary>
    Task ExecuteAsync(long scheduleId);
}
