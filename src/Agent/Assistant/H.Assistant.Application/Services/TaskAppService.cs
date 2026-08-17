using AutoMapper;
using H.Abp.Application.Contracts;
using H.Assistant.Application.Contracts;
using H.Assistant.Core;
using H.Assistant.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace H.Assistant.Application;

/// <summary>
/// 定时任务管理应用服务
/// </summary>
public class TaskAppService : ApplicationService, ITaskAppService
{
    private readonly IRepository<TaskEntity, Guid> _taskRepository;
    private readonly IRepository<TaskLogEntity, Guid> _logRepository;
    private readonly IMapper _objectMapper;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly AgentFactory _agentFactory;

    public TaskAppService(
        IRepository<TaskEntity, Guid> taskRepository,
        IRepository<TaskLogEntity, Guid> logRepository,
        IMapper objectMapper,
        IAsyncQueryableExecuter asyncExecuter,
        AgentFactory agentFactory)
    {
        _taskRepository = taskRepository;
        _logRepository = logRepository;
        _objectMapper = objectMapper;
        _asyncExecuter = asyncExecuter;
        _agentFactory = agentFactory;
    }

    public async Task<BaseOutput<PagedResultDto<TaskDto>>> GetListAsync(TaskQueryDto input)
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

        if (!string.IsNullOrWhiteSpace(input.Category))
        {
            query = query.Where(t => t.Category == input.Category);
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

        return new() { Data = new PagedResultDto<TaskDto>(totalCount, dtos) };
    }

    public async Task<BaseOutput<TaskDto>> GetAsync(Guid id)
    {
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(TaskEntity), id);
        }

        return new() { Data = _objectMapper.Map<TaskEntity, TaskDto>(task) };
    }

    public async Task<BaseOutput<TaskDto>> CreateAsync(CreateTaskDto input)
    {
        var isManual = input.ExecutionMode == "Manual";
        var task = new TaskEntity
        {
            TaskName = input.TaskName,
            TaskDescription = input.TaskDescription,
            TaskType = isManual ? "manual" : "scheduled",
            Category = input.Category,
            SourceType = string.IsNullOrWhiteSpace(input.SourceType) ? "Prompt" : input.SourceType,
            WorkflowContent = input.WorkflowContent,
            ExecutionMode = isManual ? "Manual" : "Auto",
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
            NextExecutionTime = isManual
                ? null
                : CalculateNextExecutionTime(
                    input.ScheduleType, input.CronExpression, input.Hour, input.Minute, input.DayOfWeek, input.DayOfMonth)
        };

        await _taskRepository.InsertAsync(task);

        return new() { Data = _objectMapper.Map<TaskEntity, TaskDto>(task) };
    }

    public async Task<BaseOutput<TaskDto>> UpdateAsync(Guid id, UpdateTaskDto input)
    {
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(TaskEntity), id);
        }

        var isManual = input.ExecutionMode == "Manual";
        task.TaskName = input.TaskName;
        task.TaskDescription = input.TaskDescription;
        task.TaskType = isManual ? "manual" : "scheduled";
        task.Category = input.Category;
        task.SourceType = string.IsNullOrWhiteSpace(input.SourceType) ? "Prompt" : input.SourceType;
        task.WorkflowContent = input.WorkflowContent;
        task.ExecutionMode = isManual ? "Manual" : "Auto";
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
        task.NextExecutionTime = isManual
            ? null
            : CalculateNextExecutionTime(
                input.ScheduleType, input.CronExpression, input.Hour, input.Minute, input.DayOfWeek, input.DayOfMonth);

        await _taskRepository.UpdateAsync(task);

        return new() { Data = _objectMapper.Map<TaskEntity, TaskDto>(task) };
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        // 删除关联的执行日�?
        var logQueryable = await _logRepository.GetQueryableAsync();
        var logs = await AsyncExecuter.ToListAsync(logQueryable.Where(l => l.TaskId == id));
        foreach (var log in logs)
        {
            await _logRepository.DeleteAsync(log);
        }

        await _taskRepository.DeleteAsync(id);

        return new();
    }

    public async Task<BaseOutput> ToggleEnableAsync(Guid id)
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
            // 手动任务不参与调度，无需计算下次执行时间
            task.NextExecutionTime = task.ExecutionMode == "Manual"
                ? null
                : CalculateNextExecutionTime(
                    task.ScheduleType, task.CronExpression, task.Hour, task.Minute, task.DayOfWeek, task.DayOfMonth);
        }

        await _taskRepository.UpdateAsync(task);

        return new();
    }

    public async Task<BaseOutput> ExecuteNowAsync(Guid id)
    {
        await ExecuteTaskAsync(id);
        return new();
    }

    public async Task<BaseOutput<List<TaskLogDto>>> GetExecutionLogsAsync(Guid taskId, int maxResultCount = 10)
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

        return new() { Data = logs
            .Select(l =>
            {
                var dto = _objectMapper.Map<TaskLogEntity, TaskLogDto>(l);
                dto.TaskName = taskName;
                return dto;
            })
            .ToList() };
    }

    /// <summary>
    /// 执行单个任务
    /// </summary>
    public async Task<BaseOutput> ExecuteTaskAsync(Guid taskId)
    {
        var task = await _taskRepository.FindAsync(taskId);
        if (task == null)
        {
            return new();
        }

        await ExecuteTaskInternalAsync(task);

        return new();
    }

    private async Task ExecuteTaskInternalAsync(TaskEntity task)
    {
        var startTime = DateTime.Now;
        var taskId = task.Id;
        var taskName = task.TaskName;

        Logger.LogInformation("开始执行定时任�? {TaskName} (Id={TaskId})", taskName, taskId);

        try
        {
            // 获取 Agent 实例
            IAgentInstance? agent = await _agentFactory.CreateAgentAsync(task.AgentType, task.ModelConfigId);

            if (agent == null)
            {
                throw new InvalidOperationException($"无法创建 Agent 实例: {task.AgentType}");
            }

            // 执行任务内容（提示词或工作流�?
            var response = await ExecuteTaskContentAsync(agent, task);

            // 插入成功日志
            await _logRepository.InsertAsync(new TaskLogEntity
            {
                TaskId = taskId,
                StartTime = startTime,
                EndTime = DateTime.Now,
                Status = "Success",
                Result = response
            });

            // 更新任务执行统计
            task.LastExecutionTime = DateTime.Now;
            task.ExecutionCount++;
            // 手动任务不参与调度，保持下次执行时间为空
            task.NextExecutionTime = task.ExecutionMode == "Manual"
                ? null
                : CalculateNextExecutionTime(
                    task.ScheduleType, task.CronExpression, task.Hour, task.Minute, task.DayOfWeek, task.DayOfMonth);

            if (task.ExecutionMode == "Auto" && task.ScheduleType == "Once")
            {
                task.Status = "Completed";
                task.IsEnabled = false;
            }

            await _taskRepository.UpdateAsync(task);

            Logger.LogInformation("定时任务执行成功: {TaskName} (Id={TaskId})", taskName, taskId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "定时任务执行失败: {TaskName} (Id={TaskId}), 错误: {Error}", taskName, taskId, ex.Message);

            // 插入失败日志
            try
            {
                await _logRepository.InsertAsync(new TaskLogEntity
                {
                    TaskId = taskId,
                    StartTime = startTime,
                    EndTime = DateTime.Now,
                    Status = "Failed",
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception logEx)
            {
                Logger.LogError(logEx, "记录任务失败日志时出�? TaskId={TaskId}", taskId);
            }
        }
    }

    /// <summary>
    /// 执行任务内容：根据创建方式选择提示词或工作流执�?
    /// </summary>
    private async Task<string> ExecuteTaskContentAsync(IAgentInstance agent, TaskEntity task)
    {
        // 工作流任务：按顺序执行各步骤，上一步结果作为下一步的上下�?
        if (task.SourceType == "Workflow" && !string.IsNullOrWhiteSpace(task.WorkflowContent))
        {
            var steps = JsonSerializer.Deserialize<List<WorkflowStepDto>>(task.WorkflowContent)
                ?? new List<WorkflowStepDto>();

            if (steps.Count == 0)
            {
                throw new InvalidOperationException("工作流任务未配置有效步骤");
            }

            var history = new List<string>();
            var lastResult = string.Empty;
            for (var i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var stepPrompt = string.IsNullOrWhiteSpace(step.Prompt) ? step.Name : step.Prompt;
                if (i > 0 && !string.IsNullOrEmpty(lastResult))
                {
                    stepPrompt = $"上一步骤「{steps[i - 1].Name}」的执行结果如下：\n{lastResult}\n\n请基于上述结果，继续执行当前步骤：{stepPrompt}";
                }

                Logger.LogInformation("执行工作流步�?{Index}/{Count}: {StepName} (TaskId={TaskId})",
                    i + 1, steps.Count, step.Name, task.Id);
                lastResult = await agent.ProcessMessageAsync(stepPrompt, history);
            }

            return lastResult;
        }

        // 提示词任务：直接执行提示�?
        return await agent.ProcessMessageAsync(task.PromptContent, new List<string>());
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
        var now = DateTime.Now;

        return scheduleType switch
        {
            "Once" => now, // 立即执行一�?
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
            // 今天就是目标日，检查时间是否已�?
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

        var result = new DateTime(now.Year, now.Month, day, hour ?? 0, minute ?? 0, 0, DateTimeKind.Local);

        if (result <= now)
        {
            result = result.AddMonths(1);
            day = dayOfMonth ?? 1;
            if (day > DateTime.DaysInMonth(result.Year, result.Month))
            {
                day = DateTime.DaysInMonth(result.Year, result.Month);
            }
            result = new DateTime(result.Year, result.Month, day, hour ?? 0, minute ?? 0, 0, DateTimeKind.Local);
        }

        return result;
    }

    private DateTime? ParseCronNextRun(string cronExpression, DateTime now)
    {
        // 简化实现：假设标准 Cron 格式，使�?NCrontab 库解�?
        // 这里返回一个占位时间，实际应由后台 Worker 处理
        return now.AddMinutes(1);
    }
}
