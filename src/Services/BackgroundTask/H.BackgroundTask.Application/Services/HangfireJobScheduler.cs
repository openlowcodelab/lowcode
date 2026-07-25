using H.BackgroundTask.Application.Contracts;
using H.BackgroundTask.EntityFrameworkCore;
using Hangfire;

namespace H.BackgroundTask.Application.Services;

/// <summary>
/// 基于 Hangfire 的任务调度器实现。
/// </summary>
public class HangfireJobScheduler : IJobScheduler
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireJobScheduler(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
    }

    /// <summary>周期任务的 recurring job id 约定。</summary>
    public static string RecurringId(Guid jobId) => $"bgtask:{jobId}";

    public string? Schedule(BackgroundJobEntity job)
    {
        // 先移除旧的调度，避免重复
        Remove(job);

        if (!job.IsEnabled)
        {
            return null;
        }

        if ((JobTriggerKind)job.TriggerKind == JobTriggerKind.Recurring)
        {
            if (string.IsNullOrWhiteSpace(job.CronExpression))
            {
                throw new ArgumentException("周期任务必须配置 Cron 表达式");
            }

            var recurringId = RecurringId(job.Id);
            _recurringJobManager.AddOrUpdate<IBackgroundJobExecutor>(
                recurringId,
                x => x.ExecuteAsync(job.Id),
                job.CronExpression);
            return recurringId;
        }

        // 一次性任务
        if (job.ScheduledTime.HasValue && job.ScheduledTime.Value > DateTime.Now)
        {
            return _backgroundJobClient.Schedule<IBackgroundJobExecutor>(
                x => x.ExecuteAsync(job.Id),
                new DateTimeOffset(job.ScheduledTime.Value.ToUniversalTime()));
        }

        // 未指定时间或时间已过：立即入队
        return _backgroundJobClient.Enqueue<IBackgroundJobExecutor>(x => x.ExecuteAsync(job.Id));
    }

    public void Remove(BackgroundJobEntity job)
    {
        if (string.IsNullOrWhiteSpace(job.HangfireJobId))
        {
            // 周期任务可能未记录 id，按约定 id 兜底移除
            if ((JobTriggerKind)job.TriggerKind == JobTriggerKind.Recurring)
            {
                _recurringJobManager.RemoveIfExists(RecurringId(job.Id));
            }
            return;
        }

        if ((JobTriggerKind)job.TriggerKind == JobTriggerKind.Recurring)
        {
            _recurringJobManager.RemoveIfExists(job.HangfireJobId);
        }
        else
        {
            _backgroundJobClient.Delete(job.HangfireJobId);
        }
    }

    public void Trigger(Guid jobId)
    {
        _backgroundJobClient.Enqueue<IBackgroundJobExecutor>(x => x.ExecuteAsync(jobId));
    }
}
