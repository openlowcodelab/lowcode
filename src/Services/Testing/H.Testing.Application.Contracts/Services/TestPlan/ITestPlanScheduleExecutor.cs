namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试计划定时执行器。由 Hangfire 作业服务器反射调用，需保证方法签名稳定。
/// </summary>
public interface ITestPlanScheduleExecutor
{
    /// <summary>
    /// 定时执行指定的测试计划
    /// </summary>
    Task ExecuteAsync(long planId);
}
