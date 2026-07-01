using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace H.Assistant.Application.Workers;

/// <summary>
/// 定时任务后台 Worker
/// 定期扫描待执行的定时任务并触发执行
/// </summary>
public class TaskWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // 每分钟检查一次

    public TaskWorker(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("ScheduledTaskWorker 已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskAppService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TaskWorker>>();

                var now = DateTime.Now;

                // 查找待执行的任务（已启用且下次执行时间已到）
                var tasksToExecute = await dbContext.Tasks
                    .Where(t => t.IsEnabled && t.Status == "Active" && t.NextExecutionTime <= now)
                    .ToListAsync(stoppingToken);

                if (tasksToExecute.Count > 0)
                {
                    logger.LogInformation("发现 {Count} 个待执行的定时任务", tasksToExecute.Count);

                    // 先更新所有待执行任务的下次执行时间，防止重复拾取
                    foreach (var task in tasksToExecute)
                    {
                        task.NextExecutionTime = now.AddMinutes(9999); // 设置一个远将来时间，执行完成后会重新计算
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);

                    // 逐个执行任务
                    foreach (var task in tasksToExecute)
                    {
                        try
                        {
                            logger.LogInformation("执行定时任务: {TaskName} (Id={TaskId})", task.TaskName, task.Id);
                            await taskService.ExecuteTaskAsync(task.Id);
                        }
                        catch (Exception ex)
                        {
                            // 记录错误但不中断其他任务的执行
                            logger.LogError(ex, "执行定时任务 {TaskName} (Id={TaskId}) 失败: {Error}",
                                task.TaskName, task.Id, ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ScheduledTaskWorker 检查任务失败: {ex.Message}");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }
}
