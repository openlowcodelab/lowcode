namespace H.BackgroundTask.Application.Services;

/// <summary>
/// 后台任务执行器。由 Hangfire 作业服务器反射调用，需保证方法签名稳定。
/// </summary>
public interface IBackgroundJobExecutor
{
    /// <summary>
    /// 执行指定任务：根据执行类型调用 API 或执行 SQL，并写入执行记录。
    /// </summary>
    /// <param name="jobId">任务ID</param>
    Task ExecuteAsync(Guid jobId);
}
