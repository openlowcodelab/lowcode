using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace H.Workbench.Application.Workers;

/// <summary>
/// 定时任务后台 Worker
/// 定期扫描待执行的定时任务并触发执行。
/// 抢占用条件 UPDATE（旧值精确匹配置空），单实例正确、多实例互斥；
/// 执行走独立 scope，避免 ChangeTracker 与 claim 互相串扰。
/// </summary>
public class TaskWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // 每分钟检查一次
    private bool _startupSweepDone;

    public TaskWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log("ScheduledTaskWorker 已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<WorkbenchDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TaskWorker>>();

                if (!_startupSweepDone)
                {
                    await SweepDanglingAsync(dbContext, logger, stoppingToken);
                    await SweepAbandonedRunningLogsAsync(dbContext, logger, staleBefore: DateTime.Now.AddMinutes(-2), ct: stoppingToken);
                    _startupSweepDone = true;
                }

                // 每轮兜底：回收"进程活着但流静默死掉"的超长 Running 行（断连/异常未走到收尾）
                await SweepAbandonedRunningLogsAsync(dbContext, logger, staleBefore: DateTime.Now.AddMinutes(-15), ct: stoppingToken);
                var now = DateTime.Now;

                // 查找待执行的任务（已启用且下次执行时间已到），AsNoTracking 避免干扰后续条件更新
                var due = await dbContext.Tasks.AsNoTracking()
                    .Where(t => t.IsEnabled && t.Status == "Active"
                                && t.NextExecutionTime != null && t.NextExecutionTime <= now)
                    .Select(t => new { t.Id, t.TaskName, Expected = t.NextExecutionTime })
                    .ToListAsync(stoppingToken);

                foreach (var item in due)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    // 条件 UPDATE：只有把旧值精确替换掉的实例才是赢家（被抢走则跳过）
                    var won = await dbContext.Tasks
                        .Where(x => x.Id == item.Id && x.NextExecutionTime == item.Expected
                                    && x.IsEnabled && x.Status == "Active")
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.NextExecutionTime, (DateTime?)null),
                            stoppingToken);
                    if (won == 0) continue;

                    try
                    {
                        logger.LogInformation("执行定时任务: {TaskName} (Id={TaskId})", item.TaskName, item.Id);
                        using var execScope = _serviceProvider.CreateScope();
                        var taskService = execScope.ServiceProvider.GetRequiredService<ITaskAppService>();
                        await taskService.ExecuteTaskAsync(item.Id);
                    }
                    catch (Exception ex)
                    {
                        // 记录错误但不中断其他任务的执行（ExecuteTaskAsync 内部已含失败重排）
                        logger.LogError(ex, "执行定时任务 {TaskName} (Id={TaskId}) 失败: {Error}",
                            item.TaskName, item.Id, ex.Message);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log($"ScheduledTaskWorker 检查任务失败: {ex.Message}");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// 启动清扫：回收"claim 后进程被杀"留下的悬空任务（Auto + Active + IsEnabled 且 NextExecutionTime 为空）
    /// </summary>
    private static async Task SweepDanglingAsync(WorkbenchDbContext dbContext, ILogger logger, CancellationToken ct)
    {
        var danglingIds = await dbContext.Tasks.AsNoTracking()
            .Where(t => t.IsEnabled && t.Status == "Active" && t.ExecutionMode == "Auto"
                        && t.NextExecutionTime == null)
            .Select(t => t.Id)
            .ToListAsync(ct);

        foreach (var id in danglingIds)
        {
            // 置一个已过期的时间，让本轮扫描立即接管并按正常抢占流程重排
            await dbContext.Tasks
                .Where(x => x.Id == id && x.NextExecutionTime == null)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.NextExecutionTime, DateTime.Now.AddSeconds(-1)), ct);
        }

        if (danglingIds.Count > 0)
        {
            logger.LogWarning("发现 {Count} 个悬空任务（执行中被中断），已重新排期", danglingIds.Count);
        }
    }

    /// <summary>
    /// 回收悬挂的 Running 执行日志：进程重启/流静默中断后无人回写收尾，
    /// 置为 Abandoned 保留已落库的轨迹，避免列表永远显示"执行中"
    /// </summary>
    private static async Task SweepAbandonedRunningLogsAsync(
        WorkbenchDbContext dbContext, ILogger logger, DateTime staleBefore, CancellationToken ct)
    {
        var abandoned = await dbContext.TaskLogs
            .Where(l => l.Status == "Running" && l.StartTime < staleBefore)
            .ExecuteUpdateAsync(s => s
                .SetProperty(l => l.Status, "Abandoned")
                .SetProperty(l => l.EndTime, DateTime.Now)
                .SetProperty(l => l.ErrorMessage, "执行中断（进程重启或连接断开，未经收尾）"), ct);

        if (abandoned > 0)
        {
            logger.LogWarning("回收 {Count} 条悬挂的执行中日志为 Abandoned", abandoned);
        }
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }
}
