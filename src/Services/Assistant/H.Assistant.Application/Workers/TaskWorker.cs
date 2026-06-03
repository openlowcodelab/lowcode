using H.Assistant.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp;

using H.Assistant.Application.Contracts;
namespace H.Assistant.Application.Workers;
using Microsoft.EntityFrameworkCore;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskAppService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();

                var now = DateTime.UtcNow;

                // 查找待执行的任务（已启用且下次执行时间已到）
                var tasksToExecute = await dbContext.Tasks
                    .Where(t => t.IsEnabled && t.Status == "Active" && t.NextExecutionTime <= now)
                    .ToListAsync(stoppingToken);

                foreach (var task in tasksToExecute)
                {
                    try
                    {
                        await taskService.ExecuteTaskAsync(task.Id);
                    }
                    catch (Exception ex)
                    {
                        // 记录错误但不中断其他任务的执行
                        Console.WriteLine($"执行定时任务 {task.TaskName} 失败: {ex.Message}");
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
