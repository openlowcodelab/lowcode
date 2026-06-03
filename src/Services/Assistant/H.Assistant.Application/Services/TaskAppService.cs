using System.Linq.Dynamic.Core;
using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.Core;
using H.Assistant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace H.Assistant.Application;

/// <summary>
/// 定时任务管理应用服务
/// </summary>
public class AssistantScheduledTaskAppService : ApplicationService, ITaskAppService
{
    private readonly IRepository<TaskEntity, Guid> _taskRepository;
    private readonly IRepository<TaskLogEntity, Guid> _logRepository;
    private readonly IMapper _objectMapper;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly AgentFactory _agentFactory;
    private readonly IServiceProvider _serviceProvider;

    public AssistantScheduledTaskAppService(
        IRepository<TaskEntity, Guid> taskRepository,
        IRepository<TaskLogEntity, Guid> logRepository,
        IMapper objectMapper,
        IAsyncQueryableExecuter asyncExecuter,
        AgentFactory agentFactory,
        IServiceProvider serviceProvider)
    {
        _taskRepository = taskRepository;
        _logRepository = logRepository;
        _objectMapper = objectMapper;
        _asyncExecuter = asyncExecuter;
        _agentFactory = agentFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task<PagedResultDto<TaskDto>> GetListAsync(TaskQueryDto input)
    {
        var queryable = await _taskRepository.GetQueryableAsync();

        var query = queryable.AsQueryable();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(t => t.TaskName.Contains(input.Filter) || t.TaskDescription.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(t => t.Status == input.Status);
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(t => t.IsEnabled == input.IsEnabled.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query.OrderByDescending(t => t.CreationTime);

        var tasks = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var dtos = tasks
            .Select(t => _objectMapper.Map<TaskEntity, TaskDto>(t))
            .ToList();

        return new PagedResultDto<TaskDto>(totalCount, dtos);
    }

    public async Task<TaskDto> GetAsync(Guid id)
    {
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(TaskEntity), id);
        }

        return _objectMapper.Map<TaskEntity, TaskDto>(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto input)
    {
        var task = new TaskEntity
        {
            TaskName = input.TaskName,
            TaskDescription = input.TaskDescription,
            TaskType = "scheduled",
            PromptContent = input.PromptContent,
            AgentType = input.AgentType,
            ModelConfigId = input.ModelConfigId,
            ScheduleType = input.ScheduleType,
            CronExpression = input.CronExpression,
            Hour = input.Hour,
            Minute = input.Minute,
            DayOfWeek = input.DayOfWeek,
            DayOfMonth = input.DayOfMonth,
            IsEnabled = input.IsEnabled,
            ExecutionCount = 0,
            Status = "Active",
            NextExecutionTime = CalculateNextExecutionTime(
                input.ScheduleType, input.CronExpression, input.Hour, input.Minute, input.DayOfWeek, input.DayOfMonth)
        };

        await _taskRepository.InsertAsync(task);

        return _objectMapper.Map<TaskEntity, TaskDto>(task);
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskDto input)
    {
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(TaskEntity), id);
        }

        task.TaskName = input.TaskName;
        task.TaskDescription = input.TaskDescription;
        task.PromptContent = input.PromptContent;
        task.AgentType = input.AgentType;
        task.ModelConfigId = input.ModelConfigId;
        task.ScheduleType = input.ScheduleType;
        task.CronExpression = input.CronExpression;
        task.Hour = input.Hour;
        task.Minute = input.Minute;
        task.DayOfWeek = input.DayOfWeek;
        task.DayOfMonth = input.DayOfMonth;
        task.IsEnabled = input.IsEnabled;
        task.NextExecutionTime = CalculateNextExecutionTime(
            input.ScheduleType, input.CronExpression, input.Hour, input.Minute, input.DayOfWeek, input.DayOfMonth);

        await _taskRepository.UpdateAsync(task);

        return _objectMapper.Map<TaskEntity, TaskDto>(task);
    }

    public async Task DeleteAsync(Guid id)
    {
        // 删除关联的执行日志
        var logQueryable = await _logRepository.GetQueryableAsync();
        var logs = await AsyncExecuter.ToListAsync(logQueryable.Where(l => l.TaskId == id));
        foreach (var log in logs)
        {
            await _logRepository.DeleteAsync(log);
        }

        await _taskRepository.DeleteAsync(id);
    }

    public async Task ToggleEnableAsync(Guid id)
    {
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(TaskEntity), id);
        }

        task.IsEnabled = !task.IsEnabled;

        if (!task.IsEnabled)
        {
            task.Status = "Paused";
        }
        else if (task.Status == "Paused")
        {
            task.Status = "Active";
            task.NextExecutionTime = CalculateNextExecutionTime(
                task.ScheduleType, task.CronExpression, task.Hour, task.Minute, task.DayOfWeek, task.DayOfMonth);
        }

        await _taskRepository.UpdateAsync(task);
    }

    public async Task ExecuteNowAsync(Guid id)
    {
        await ExecuteTaskAsync(id);
    }

    public async Task<List<TaskLogDto>> GetExecutionLogsAsync(Guid taskId, int maxResultCount = 10)
    {
        var queryable = await _logRepository.GetQueryableAsync();

        var logs = await AsyncExecuter.ToListAsync(
            queryable
                .Where(l => l.TaskId == taskId)
                .OrderByDescending(l => l.StartTime)
                .Take(maxResultCount)
        );

        // 获取任务名称
        var task = await _taskRepository.FindAsync(taskId);
        var taskName = task?.TaskName ?? string.Empty;

        return logs
            .Select(l =>
            {
                var dto = _objectMapper.Map<TaskLogEntity, TaskLogDto>(l);
                dto.TaskName = taskName;
                return dto;
            })
            .ToList();
    }

    /// <summary>
    /// 执行单个任务
    /// </summary>
    public async Task ExecuteTaskAsync(Guid taskId)
    {
        var task = await _taskRepository.FindAsync(taskId);
        if (task == null)
        {
            return;
        }

        await ExecuteTaskInternalAsync(task);
    }

    private async Task ExecuteTaskInternalAsync(TaskEntity task)
    {
        var startTime = DateTime.UtcNow;
        var taskId = task.Id;
        var taskName = task.TaskName;
        var promptContent = task.PromptContent;
        var agentType = task.AgentType;
        var modelConfigId = task.ModelConfigId;
        var scheduleType = task.ScheduleType;
        var cronExpression = task.CronExpression;
        var hour = task.Hour;
        var minute = task.Minute;
        var dayOfWeek = task.DayOfWeek;
        var dayOfMonth = task.DayOfMonth;

        // 异步后台执行任务 - 在 Task.Run 内部创建新的 scope
        _ = Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var logRepository = scope.ServiceProvider.GetRequiredService<IRepository<TaskLogEntity, Guid>>();
                var taskRepository = scope.ServiceProvider.GetRequiredService<IRepository<TaskEntity, Guid>>();
                var agentFactory = scope.ServiceProvider.GetRequiredService<AgentFactory>();

                // 获取 Agent 实例
                IAgentInstance? agent = await agentFactory.CreateAgentAsync(agentType, modelConfigId);
                
                if (agent == null)
                {
                    throw new InvalidOperationException($"无法创建 Agent 实例: {agentType}");
                }

                // 执行 Prompt
                var response = await agent.ProcessMessageAsync(promptContent, new List<string>());

                // 插入成功日志
                await logRepository.InsertAsync(new TaskLogEntity
                {
                    TaskId = taskId,
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Status = "Success",
                    Result = response
                });

                // 更新任务执行统计
                var currentTask = await taskRepository.FindAsync(taskId);
                if (currentTask != null)
                {
                    currentTask.LastExecutionTime = DateTime.UtcNow;
                    currentTask.ExecutionCount++;
                    currentTask.NextExecutionTime = CalculateNextExecutionTime(
                        scheduleType, cronExpression, hour, minute, dayOfWeek, dayOfMonth);

                    if (scheduleType == "Once")
                    {
                        currentTask.Status = "Completed";
                        currentTask.IsEnabled = false;
                    }

                    await taskRepository.UpdateAsync(currentTask);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    var logRepository = scope.ServiceProvider.GetRequiredService<IRepository<TaskLogEntity, Guid>>();
                    // 插入失败日志
                    await logRepository.InsertAsync(new TaskLogEntity
                    {
                        TaskId = taskId,
                        StartTime = startTime,
                        EndTime = DateTime.UtcNow,
                        Status = "Failed",
                        ErrorMessage = ex.Message
                    });
                }
                catch
                {
                    // 忽略日志记录失败
                }
            }
        });
    }

    /// <summary>
    /// 更新任务执行统计信息
    /// </summary>
    private async Task UpdateTaskExecutionStatsAsync(TaskEntity task)
    {
        task.LastExecutionTime = DateTime.UtcNow;
        task.ExecutionCount++;

        // 计算下次执行时间
        task.NextExecutionTime = CalculateNextExecutionTime(
            task.ScheduleType, task.CronExpression, task.Hour, task.Minute, task.DayOfWeek, task.DayOfMonth);

        // 如果是一次性任务,标记为完成
        if (task.ScheduleType == "Once")
        {
            task.Status = "Completed";
            task.IsEnabled = false;
        }

        await _taskRepository.UpdateAsync(task);
    }

    /// <summary>
    /// 计算下次执行时间
    /// </summary>
    private DateTime? CalculateNextExecutionTime(
        string scheduleType,
        string? cronExpression,
        int? hour,
        int? minute,
        int? dayOfWeek,
        int? dayOfMonth)
    {
        var now = DateTime.UtcNow;

        return scheduleType switch
        {
            "Once" => now, // 立即执行一次
            "Daily" => CreateDailyDateTime(now, hour, minute),
            "Weekly" => CreateWeeklyDateTime(now, dayOfWeek, hour, minute),
            "Monthly" => CreateMonthlyDateTime(now, dayOfMonth, hour, minute),
            "Cron" => cronExpression != null ? ParseCronNextRun(cronExpression, now) : null,
            _ => null
        };
    }

    private DateTime CreateDailyDateTime(DateTime now, int? hour, int? minute)
    {
        var h = hour ?? 0;
        var m = minute ?? 0;
        var result = now.Date.AddHours(h).AddMinutes(m);

        if (result <= now)
        {
            result = result.AddDays(1);
        }

        return result;
    }

    private DateTime CreateWeeklyDateTime(DateTime now, int? dayOfWeek, int? hour, int? minute)
    {
        var targetDay = dayOfWeek ?? 1; // 默认周一
        var currentDay = (int)now.DayOfWeek;
        var daysUntilTarget = (targetDay - currentDay + 7) % 7;

        if (daysUntilTarget == 0)
        {
            // 今天就是目标日，检查时间是否已过
            var result = now.Date.AddHours(hour ?? 0).AddMinutes(minute ?? 0);
            if (result <= now)
            {
                result = result.AddDays(7);
            }
            return result;
        }

        var nextDate = now.Date.AddDays(daysUntilTarget);
        return nextDate.AddHours(hour ?? 0).AddMinutes(minute ?? 0);
    }

    private DateTime CreateMonthlyDateTime(DateTime now, int? dayOfMonth, int? hour, int? minute)
    {
        var day = dayOfMonth ?? 1;
        if (day > DateTime.DaysInMonth(now.Year, now.Month))
        {
            day = DateTime.DaysInMonth(now.Year, now.Month);
        }

        var result = new DateTime(now.Year, now.Month, day, hour ?? 0, minute ?? 0, 0, DateTimeKind.Utc);

        if (result <= now)
        {
            result = result.AddMonths(1);
            day = dayOfMonth ?? 1;
            if (day > DateTime.DaysInMonth(result.Year, result.Month))
            {
                day = DateTime.DaysInMonth(result.Year, result.Month);
            }
            result = new DateTime(result.Year, result.Month, day, hour ?? 0, minute ?? 0, 0, DateTimeKind.Utc);
        }

        return result;
    }

    private DateTime? ParseCronNextRun(string cronExpression, DateTime now)
    {
        // 简化实现：假设标准 Cron 格式，使用 NCrontab 库解析
        // 这里返回一个占位时间，实际应由后台 Worker 处理
        return now.AddMinutes(1);
    }
}
