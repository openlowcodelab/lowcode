using H.BackgroundTask.EntityFrameworkCore;

namespace H.BackgroundTask.Application.Services;

/// <summary>
/// 任务调度器抽象：封装 Hangfire 的作业注册/移除/触发，隔离应用服务对具体调度实现的依赖。
/// </summary>
public interface IJobScheduler
{
    /// <summary>
    /// 根据任务定义注册或更新调度，返回 Hangfire 作业标识。
    /// 一次性任务返回 background job id；周期任务返回 recurring job id。
    /// </summary>
    string? Schedule(BackgroundJobEntity job);

    /// <summary>移除任务对应的 Hangfire 作业。</summary>
    void Remove(BackgroundJobEntity job);

    /// <summary>立即触发一次执行。</summary>
    void Trigger(Guid jobId);
}
