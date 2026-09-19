using AutoMapper;
using H.Abp.Application.Contracts;
using H.Workbench.Application.Contracts;
using H.Workbench.Core;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.Extensions.Logging;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace H.Workbench.Application;

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

        if (!string.IsNullOrWhiteSpace(input.AgentType))
        {
            query = query.Where(t => t.AgentType == input.AgentType);
        }

        if (!string.IsNullOrWhiteSpace(input.TaskType))
        {
            query = query.Where(t => t.TaskType == input.TaskType);
        }

        if (input.ProjectId.HasValue)
        {
            query = query.Where(t => t.ProjectId == input.ProjectId.Value);
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

        return new(new PagedResultDto<TaskDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<TaskDto>> GetAsync(Guid id)
    {
        var task = await _taskRepository.FindAsync(id);
        if (task == null)
        {
            throw new EntityNotFoundException(typeof(TaskEntity), id);
        }

        return new(_objectMapper.Map<TaskEntity, TaskDto>(task));
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
            ProjectId = input.ProjectId,
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
                : CalculateAndValidateNext(
                    input.ScheduleType, input.CronExpression, input.Hour, input.Minute, input.DayOfWeek, input.DayOfMonth)
        };

        task = await _taskRepository.InsertAsync(task);

        return new(_objectMapper.Map<TaskEntity, TaskDto>(task));
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
        task.ProjectId = input.ProjectId;
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
            : CalculateAndValidateNext(
                input.ScheduleType, input.CronExpression, input.Hour, input.Minute, input.DayOfWeek, input.DayOfMonth);

        task = await _taskRepository.UpdateAsync(task);

        return new(_objectMapper.Map<TaskEntity, TaskDto>(task));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        // 删除关联的执行日志
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

    public async IAsyncEnumerable<string> ExecuteStreamAsync(ExecuteTaskStreamInputDto input)
    {
        var task = await _taskRepository.FindAsync(input.TaskId);
        if (task == null)
        {
            yield return SerializeError($"任务不存在: {input.TaskId}");
            yield break;
        }

        var prompt = string.IsNullOrWhiteSpace(input.Prompt) ? task.PromptContent : input.Prompt.Trim();

        IAgentInstance? agent = null;
        string? agentError = null;
        try
        {
            agent = await _agentFactory.CreateAgentAsync(task.AgentType, task.ModelConfigId, new AgentRunContext(task.ProjectId));
        }
        catch (Exception ex)
        {
            agentError = ex.Message;
        }

        if (agent == null)
        {
            yield return SerializeError(agentError ?? $"无法创建员工实例: {task.AgentType}");
            yield break;
        }

        var startTime = DateTime.Now;
        var thinking = new StringBuilder();
        var answer = new StringBuilder();
        string? failure = null;

        // 续聊上下文：带最近若干次成功执行的问答对
        var history = await BuildConversationHistoryAsync(task);

        if (task.SourceType == "Workflow" && !string.IsNullOrWhiteSpace(task.WorkflowContent))
        {
            // 工作流任务无法增量输出，整体执行后以单个 answer 事件返回
            string? response = null;
            try
            {
                response = await ExecuteTaskContentAsync(agent, task);
            }
            catch (Exception ex)
            {
                failure = ex.Message;
            }

            if (response is not null)
            {
                answer.Append(response);
                yield return SerializeAnswer(response);
            }
        }
        else if (agent is IStreamingAgent streamingAgent)
        {
            await using var enumerator = streamingAgent.ProcessMessageStreamAsync(prompt, history).GetAsyncEnumerator();
            while (true)
            {
                bool hasNext;
                string? chunk = null;
                try
                {
                    hasNext = await enumerator.MoveNextAsync();
                    if (hasNext)
                    {
                        chunk = enumerator.Current;
                    }
                }
                catch (Exception ex)
                {
                    failure = ex.Message;
                    hasNext = false;
                }

                if (!hasNext)
                {
                    break;
                }

                AccumulateStreamEvent(chunk!, thinking, answer, ref failure);
                yield return chunk!;
            }
        }
        else
        {
            string? response = null;
            try
            {
                response = await agent.ProcessMessageAsync(prompt, new List<string>());
            }
            catch (Exception ex)
            {
                failure = ex.Message;
            }

            if (response is not null)
            {
                answer.Append(response);
                yield return SerializeAnswer(response);
            }
        }

        // 与同步执行口径一致：answer 优先，否则回退累积的 thinking 增量
        var finalAnswer = answer.Length > 0 ? answer.ToString() : thinking.ToString();
        var succeeded = failure is null && finalAnswer.Length > 0;

        if (failure is not null)
        {
            yield return SerializeError(failure);
        }

        try
        {
            await _logRepository.InsertAsync(new TaskLogEntity
            {
                TaskId = task.Id,
                Prompt = prompt,
                StartTime = startTime,
                EndTime = DateTime.Now,
                Status = succeeded ? "Success" : "Failed",
                Result = succeeded ? finalAnswer : null,
                ErrorMessage = failure
            });

            if (succeeded)
            {
                task.LastExecutionTime = DateTime.Now;
                task.ExecutionCount++;
                if (task.ExecutionMode == "Manual")
                {
                    task.NextExecutionTime = null;
                }
                await _taskRepository.UpdateAsync(task);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "流式任务执行结果落库失败 TaskId={TaskId}", task.Id);
        }
    }

    /// <summary>
    /// 从最近 10 次成功执行构建 "user:/assistant:" 对话历史（续聊上下文）
    /// </summary>
    private async Task<List<string>> BuildConversationHistoryAsync(TaskEntity task)
    {
        var history = new List<string>();
        try
        {
            var queryable = await _logRepository.GetQueryableAsync();
            var recent = await AsyncExecuter.ToListAsync(
                queryable.Where(l => l.TaskId == task.Id && l.Status == "Success" && l.Result != null)
                    .OrderByDescending(l => l.StartTime)
                    .Take(10));

            recent.Reverse();
            foreach (var log in recent)
            {
                history.Add($"user: {log.Prompt ?? task.PromptContent}");
                history.Add($"assistant: {log.Result}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "构建任务对话历史失败 TaskId={TaskId}", task.Id);
        }

        return history;
    }

    private static void AccumulateStreamEvent(string chunk, StringBuilder thinking, StringBuilder answer, ref string? failure)
    {
        try
        {
            using var doc = JsonDocument.Parse(chunk);
            var root = doc.RootElement;
            var type = root.TryGetProperty("type", out var t) ? t.GetString() : null;
            switch (type)
            {
                case "thinking":
                    if (root.TryGetProperty("content", out var c)) thinking.Append(c.GetString());
                    break;
                case "answer":
                    if (root.TryGetProperty("content", out var a)) answer.Append(a.GetString());
                    break;
                case "error":
                    var isFatal = !root.TryGetProperty("isFatal", out var f) || f.GetBoolean();
                    if (isFatal && root.TryGetProperty("message", out var m)) failure = m.GetString();
                    break;
            }
        }
        catch
        {
            // 非 JSON 负载（旧格式）按纯文本增量累积
            thinking.Append(chunk);
        }
    }

    private static string SerializeError(string message)
        => JsonSerializer.Serialize(new { type = "error", message, isFatal = true });

    private static string SerializeAnswer(string content)
        => JsonSerializer.Serialize(new { type = "answer", content, iteration = 0 });

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

        return new(logs
            .Select(l =>
            {
                var dto = _objectMapper.Map<TaskLogEntity, TaskLogDto>(l);
                dto.TaskName = taskName;
                return dto;
            })
            .ToList());
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

        Logger.LogInformation("开始执行定时任务 {TaskName} (Id={TaskId})", taskName, taskId);

        try
        {
            // 获取 Agent 实例（携带任务级项目上下文，用于注入仓库清单）
            IAgentInstance? agent = await _agentFactory.CreateAgentAsync(
                task.AgentType, task.ModelConfigId, new AgentRunContext(task.ProjectId));

            if (agent == null)
            {
                throw new InvalidOperationException($"无法创建 Agent 实例: {task.AgentType}");
            }

            // 执行任务内容（提示词或工作流）
            var response = await ExecuteTaskContentAsync(agent, task);

            // 插入成功日志
            await _logRepository.InsertAsync(new TaskLogEntity
            {
                TaskId = taskId,
                Prompt = task.PromptContent,
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
                    Prompt = task.PromptContent,
                    StartTime = startTime,
                    EndTime = DateTime.Now,
                    Status = "Failed",
                    ErrorMessage = ex.Message
                });
            }
            catch (Exception logEx)
            {
                Logger.LogError(logEx, "记录任务失败日志时出错 TaskId={TaskId}", taskId);
            }

            await RescheduleAfterFailureAsync(task);
        }
    }

    /// <summary>
    /// 失败后重排：NextExecutionTime 为空说明已被 Worker 抢占（或 Once 首跑），
    /// 必须重新给出下次时间，否则任务静默丢失；Once 连续 3 次失败后停用。
    /// </summary>
    private async Task RescheduleAfterFailureAsync(TaskEntity task)
    {
        if (task.ExecutionMode != "Auto" || task.NextExecutionTime != null) return;

        try
        {
            if (task.ScheduleType == "Once")
            {
                var successQuery = await _logRepository.GetQueryableAsync();
                var lastSuccess = await _asyncExecuter.FirstOrDefaultAsync(
                    successQuery.Where(l => l.TaskId == task.Id && l.Status == "Success")
                        .OrderByDescending(l => l.StartTime)
                        .Select(l => (DateTime?)l.StartTime));

                var failedQuery = await _logRepository.GetQueryableAsync();
                var previousFailures = await _asyncExecuter.CountAsync(
                    failedQuery.Where(l => l.TaskId == task.Id && l.Status == "Failed"
                                        && l.StartTime > (lastSuccess ?? DateTime.MinValue)));

                // 本次失败尚未落库可见，计 +1
                if (previousFailures + 1 >= 3)
                {
                    task.Status = "Failed";
                    task.IsEnabled = false;
                    task.NextExecutionTime = null;
                    Logger.LogWarning("Once 任务连续 {Count} 次失败，已停用: {TaskName} (Id={TaskId})",
                        previousFailures + 1, task.TaskName, task.Id);
                }
                else
                {
                    task.NextExecutionTime = DateTime.Now.AddMinutes(5);
                }
            }
            else
            {
                // 周期任务失败：5 分钟后退避重试一次，成功后恢复原周期
                task.NextExecutionTime = DateTime.Now.AddMinutes(5);
            }

            await _taskRepository.UpdateAsync(task);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "任务失败后重排下次执行时间出错 TaskId={TaskId}", task.Id);
        }
    }

    /// <summary>
    /// 执行任务内容：根据创建方式选择提示词或工作流执行
    /// </summary>
    private async Task<string> ExecuteTaskContentAsync(IAgentInstance agent, TaskEntity task)
    {
        // 工作流任务：按顺序执行各步骤，上一步结果作为下一步的上下文
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

                Logger.LogInformation("执行工作流步骤 {Index}/{Count}: {StepName} (TaskId={TaskId})",
                    i + 1, steps.Count, step.Name, task.Id);
                lastResult = await agent.ProcessMessageAsync(stepPrompt, history);
            }

            return lastResult;
        }

        // 提示词任务：直接执行提示词
        return await agent.ProcessMessageAsync(task.PromptContent, new List<string>());
    }

    /// <summary>
    /// 计算下次执行时间；算不出即抛校验异常（保存时立即报错，而不是运行时静默）
    /// </summary>
    private DateTime? CalculateAndValidateNext(
        string scheduleType,
        string? cronExpression,
        int? hour,
        int? minute,
        int? dayOfWeek,
        int? dayOfMonth)
    {
        var next = CalculateNextExecutionTime(scheduleType, cronExpression, hour, minute, dayOfWeek, dayOfMonth);
        if (next == null)
        {
            throw new Volo.Abp.Validation.AbpValidationException(
                $"无法计算下次执行时间，请检查调度配置（类型: {scheduleType}" +
                (scheduleType == "Cron" ? $"，表达式: {cronExpression}" : "") + "）",
                new List<System.ComponentModel.DataAnnotations.ValidationResult>());
        }

        return next;
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

    private DateTime? ParseCronNextRun(string? cronExpression, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(cronExpression)) return null;

        var trimmed = cronExpression.Trim();
        if (!Cronos.CronExpression.TryParse(trimmed, out var cron) &&
            !Cronos.CronExpression.TryParse(trimmed, Cronos.CronFormat.IncludeSeconds, out cron))
        {
            Logger.LogWarning("非法 Cron 表达式: {Cron}", cronExpression);
            return null;
        }

        // 起点 +1 秒：避免"当前时刻恰好命中"导致同一分钟内连续触发；
        // Cronos 约定 zone 重载必须传 UTC，结果转回本地时间与其余调度字段口径一致
        var fromUtc = now.AddSeconds(1).ToUniversalTime();
        var nextUtc = cron!.GetNextOccurrence(fromUtc, TimeZoneInfo.Local);
        return nextUtc?.ToLocalTime();
    }
}
