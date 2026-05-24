using System.Linq.Dynamic.Core;
using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace H.Assistant.Application;

/// <summary>
/// 定时任务管理应用服务
/// </summary>
public class AssistantScheduledTaskAppService : ApplicationService, IScheduledTaskAppService
{
    private readonly IRepository<ScheduledTaskEntity, Guid> _taskRepository;
    private readonly IRepository<TaskExecutionLogEntity, Guid> _logRepository;
    private readonly IAssistantChatAppService _chatAppService;
    private readonly IMapper _objectMapper;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public AssistantScheduledTaskAppService(
        IRepository<ScheduledTaskEntity, Guid> taskRepository,
        IRepository<TaskExecutionLogEntity, Guid> logRepository,
        IAssistantChatAppService chatAppService,
        IMapper objectMapper,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _taskRepository = taskRepository;
        _logRepository = logRepository;
        _chatAppService = chatAppService;
        _objectMapper = objectMapper;
        _asyncExecuter = asyncExecuter;
    }

    public async Task<PagedResultDto<ScheduledTaskDto>> GetListAsync(ScheduledTaskQueryDto input)
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
            .Select(t => _objectMapper.Map<ScheduledTaskEntity, ScheduledTaskDto>(t))
            .ToList();

        return new PagedResultDto<ScheduledTaskDto>(totalCount, dtos);
    }

    public async Task<ScheduledTaskDto> GetAsync(Guid id)
    {
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(ScheduledTaskEntity), id);
        }

        return _objectMapper.Map<ScheduledTaskEntity, ScheduledTaskDto>(task);
    }

    public async Task<ScheduledTaskDto> CreateAsync(CreateScheduledTaskInputDto input)
    {
        var task = new ScheduledTaskEntity
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

        return _objectMapper.Map<ScheduledTaskEntity, ScheduledTaskDto>(task);
    }

    public async Task<ScheduledTaskDto> UpdateAsync(Guid id, UpdateScheduledTaskInputDto input)
    {
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(ScheduledTaskEntity), id);
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

        return _objectMapper.Map<ScheduledTaskEntity, ScheduledTaskDto>(task);
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
            throw new EntityNotFoundException(typeof(ScheduledTaskEntity), id);
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
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(ScheduledTaskEntity), id);
        }

        await ExecuteTaskAsync(task.Id);
    }

    public async Task<List<TaskExecutionLogDto>> GetExecutionLogsAsync(Guid taskId, int maxResultCount = 10)
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
                var dto = _objectMapper.Map<TaskExecutionLogEntity, TaskExecutionLogDto>(l);
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
            throw new EntityNotFoundException(typeof(ScheduledTaskEntity), taskId);
        }

        await ExecuteTaskInternalAsync(task);
    }

    private async Task ExecuteTaskInternalAsync(ScheduledTaskEntity task)
    {
        
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
