using AutoMapper;
using H.Abp.Application.Contracts;
using H.Workbench.Application.Contracts;
using H.Workbench.Application.Services.Execution;
using H.Workbench.Core;
using H.Workbench.Core.Agents;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly IRepository<TaskExecutionStepEntity, Guid> _stepRepository;
    private readonly IRepository<ArtifactEntity, Guid> _artifactRepository;
    private readonly IMapper _objectMapper;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly AgentFactory _agentFactory;
    private readonly ExecutionTraceStore _traceStore;
    private readonly ApprovalGateway _approvalGateway;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly WorkbenchToolOptions _toolOptions;

    public TaskAppService(
        IRepository<TaskEntity, Guid> taskRepository,
        IRepository<TaskLogEntity, Guid> logRepository,
        IRepository<TaskExecutionStepEntity, Guid> stepRepository,
        IRepository<ArtifactEntity, Guid> artifactRepository,
        IMapper objectMapper,
        IAsyncQueryableExecuter asyncExecuter,
        AgentFactory agentFactory,
        ExecutionTraceStore traceStore,
        ApprovalGateway approvalGateway,
        IHttpContextAccessor httpContextAccessor,
        IOptions<WorkbenchToolOptions> toolOptions)
    {
        _taskRepository = taskRepository;
        _logRepository = logRepository;
        _stepRepository = stepRepository;
        _artifactRepository = artifactRepository;
        _objectMapper = objectMapper;
        _asyncExecuter = asyncExecuter;
        _agentFactory = agentFactory;
        _traceStore = traceStore;
        _approvalGateway = approvalGateway;
        _httpContextAccessor = httpContextAccessor;
        _toolOptions = toolOptions.Value;
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
        // 删除关联的执行日志及其轨迹/产物（表间无 FK，级联由应用层负责）
        var logQueryable = await _logRepository.GetQueryableAsync();
        var logs = await AsyncExecuter.ToListAsync(logQueryable.Where(l => l.TaskId == id));
        foreach (var log in logs)
        {
            await DeleteTraceAndArtifactsAsync(log.Id);
            await _logRepository.DeleteAsync(log);
        }

        await _taskRepository.DeleteAsync(id);

        return new();
    }

    private async Task DeleteTraceAndArtifactsAsync(Guid taskLogId)
    {
        var stepQueryable = await _stepRepository.GetQueryableAsync();
        var steps = await AsyncExecuter.ToListAsync(stepQueryable.Where(s => s.TaskLogId == taskLogId));
        if (steps.Count > 0)
        {
            await _stepRepository.DeleteManyAsync(steps, autoSave: true);
        }

        var artifactQueryable = await _artifactRepository.GetQueryableAsync();
        var artifacts = await AsyncExecuter.ToListAsync(artifactQueryable.Where(a => a.TaskLogId == taskLogId));
        if (artifacts.Count > 0)
        {
            await _artifactRepository.DeleteManyAsync(artifacts, autoSave: true);
        }
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

        var startTime = DateTime.Now;
        var thinking = new StringBuilder();
        var answer = new StringBuilder();
        string? failure = null;

        // 客户端断连检测：SSE 连接断开后终止 ReAct 循环，防审批挂到超时
        var ct = _httpContextAccessor.HttpContext?.RequestAborted ?? CancellationToken.None;

        // 续聊上下文：带最近若干次成功执行的问答对（必须在 Running 日志插入前构建，
        // 且只取 Status=Success 行，轨迹落库不影响该口径）
        var history = await BuildConversationHistoryAsync(task);

        // 起始即插 Running 行（独立短 UoW 立即提交，见 StartLogAsync 注释）：轨迹/产物需要立即存在的宿主
        var logId = Guid.NewGuid();
        try
        {
            await _traceStore.StartLogAsync(logId, task.Id, prompt, startTime);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "插入 Running 执行日志失败 TaskId={TaskId}", task.Id);
        }

        // 审批模式：流式对话任务可回传裁决；工作流卡点同样走审批通道
        var isWorkflow = task.SourceType == "Workflow" && !string.IsNullOrWhiteSpace(task.WorkflowContent);
        var approvalMode = isWorkflow ? "Interactive" : "Interactive";
        List<WorkflowStepDto>? steps = null;
        if (isWorkflow)
        {
            steps = ParseWorkflowSteps(task.WorkflowContent!);
        }

        IAgentInstance? agent = null;
        string? agentError = null;
        try
        {
            agent = await _agentFactory.CreateAgentAsync(task.AgentType, task.ModelConfigId,
                new AgentRunContext(task.ProjectId, null, approvalMode, task.Id, logId, CurrentUser.Id?.ToString()));
        }
        catch (Exception ex)
        {
            agentError = ex.Message;
        }

        if (agent == null)
        {
            agentError ??= $"无法创建员工实例: {task.AgentType}";
            try
            {
                await _traceStore.CompleteLogAsync(logId, "Failed", null, agentError, 0, 0, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "回写失败日志出错 TaskId={TaskId}", task.Id);
            }
            yield return SerializeError(agentError);
            yield break;
        }

        var recorder = new ExecutionTraceRecorder(_traceStore, _toolOptions.Trace, Logger, logId, task.Id);
        var completedNormally = false;

        // 迭代器规则：yield 不得位于带 catch 的 try 内；收尾放 finally，
        // 客户端断连（消费者 DisposeAsync）时也能执行，把 Running 行落成 Cancelled
        try
        {
            if (isWorkflow)
            {
                // 工作流：引擎在后台跑（步骤边界/卡点审批事件经 Channel 桥接实时转发），
                // 步骤过程由 recorder 直接落库，前端据 step 事件构建分组时间线
                var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();
                WorkflowRunSummaryDto? wfSummary = null;
                Exception? wfError = null;

                async Task RunEngineAsync()
                {
                    try
                    {
                        wfSummary = await RunWorkflowAsync(
                            task, steps!, recorder,
                            BuildStepAgentResolver(task, agent, logId, "Interactive"),
                            async json => await channel.Writer.WriteAsync(json, ct),
                            interactive: true, logId, ct);
                    }
                    catch (Exception ex)
                    {
                        wfError = ex;
                    }
                    finally
                    {
                        channel.Writer.TryComplete();
                    }
                }

                var engine = RunEngineAsync();
                await foreach (var evt in channel.Reader.ReadAllAsync())
                {
                    await recorder.HandleAsync(evt);
                    yield return evt;
                }

                await Task.WhenAll(engine);
                if (wfError is not null)
                {
                    failure = wfError.Message;
                }
                else if (wfSummary is not null)
                {
                    var md = BuildWorkflowAnswer(wfSummary);
                    answer.Append(md);
                    yield return SerializeAnswer(md);
                }
            }
            else if (agent is IStreamingAgent streamingAgent)
            {
                await using var enumerator = streamingAgent.ProcessMessageStreamAsync(prompt, history, ct).GetAsyncEnumerator();
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
                    await recorder.HandleAsync(chunk!);
                    yield return chunk!;
                }
            }
            else
            {
                string? response = null;
                try
                {
                    response = await agent.ProcessMessageAsync(prompt, new List<string>(), recorder.Tap);
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

            if (failure is not null && !ct.IsCancellationRequested)
            {
                yield return SerializeError(failure);
            }

            completedNormally = true;
        }
        finally
        {
            // 与同步执行口径一致：answer 优先，否则回退累积的 thinking 增量
            var finalAnswer = answer.Length > 0 ? answer.ToString() : thinking.ToString();
            var aborted = !completedNormally || ct.IsCancellationRequested;
            var succeeded = !aborted && failure is null && finalAnswer.Length > 0;

            try
            {
                var (stepCount, artifactCount, approvalState) = await recorder.FinishAsync();

                await _traceStore.CompleteLogAsync(
                    logId,
                    aborted ? "Cancelled" : succeeded ? "Success" : "Failed",
                    succeeded ? finalAnswer : null,
                    aborted ? "客户端断开，执行中止" : failure,
                    stepCount,
                    artifactCount,
                    approvalState);

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

    public async Task<BaseOutput<TaskLogTraceDto>> GetLogTraceAsync(Guid logId, int maxStepCount = 200)
    {
        var log = await _logRepository.FindAsync(logId);
        if (log == null)
        {
            return new BaseOutput<TaskLogTraceDto>
            {
                Code = 1,
                Success = false,
                Message = "执行记录不存在"
            };
        }

        var stepQueryable = await _stepRepository.GetQueryableAsync();
        var steps = await AsyncExecuter.ToListAsync(
            stepQueryable.Where(s => s.TaskLogId == logId)
                .OrderBy(s => s.Iteration).ThenBy(s => s.Seq)
                .Take(maxStepCount));

        var artifactQueryable = await _artifactRepository.GetQueryableAsync();
        var artifacts = await AsyncExecuter.ToListAsync(
            artifactQueryable.Where(a => a.TaskLogId == logId)
                .OrderBy(a => a.CreationTime));

        var trace = new TaskLogTraceDto
        {
            Status = log.Status,
            Steps = steps.Select(s => _objectMapper.Map<TaskExecutionStepEntity, ExecutionStepDto>(s)).ToList(),
            Artifacts = artifacts.Select(a => _objectMapper.Map<ArtifactEntity, ArtifactDto>(a)).ToList()
        };

        return new(trace);
    }

    public async Task<BaseOutput<ApprovalOutcomeDto>> ResumeApprovalAsync(ResumeApprovalInputDto input)
    {
        var userId = CurrentUser.Id?.ToString();
        var completed = _approvalGateway.TryComplete(input.ApprovalId, input.Approved, userId, out var result);

        var dto = new ApprovalOutcomeDto
        {
            ApprovalId = input.ApprovalId,
            Decision = completed ? result.Outcome.ToString() : ApprovalOutcome.Unknown.ToString()
        };

        if (!completed)
        {
            dto.Message = "审批请求已过期或已被裁决，请重新执行任务";
            return new(dto) { Success = false, Code = 1 };
        }

        return new(dto);
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

        // 后台执行同样先立 Running 行（独立短 UoW，避免外层事务性 UoW 导致收尾时查不到行）
        var logId = Guid.NewGuid();
        ExecutionTraceRecorder? recorder = null;
        try
        {
            await _traceStore.StartLogAsync(logId, taskId, task.PromptContent, startTime);

            recorder = new ExecutionTraceRecorder(_traceStore, _toolOptions.Trace, Logger, logId, taskId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "插入 Running 执行日志失败 TaskId={TaskId}", taskId);
        }

        try
        {
            // 获取 Agent 实例（携带任务级项目上下文与审批策略：后台执行无法人工裁决，按配置自动裁决）
            IAgentInstance? agent = await _agentFactory.CreateAgentAsync(
                task.AgentType, task.ModelConfigId,
                new AgentRunContext(task.ProjectId, null, "NonInteractive", taskId, logId, CurrentUser.Id?.ToString()));

            if (agent == null)
            {
                throw new InvalidOperationException($"无法创建 Agent 实例: {task.AgentType}");
            }

            // 执行任务内容（提示词或工作流），事件流经 recorder 落轨迹
            string response;
            string? workflowFailure = null;
            if (task.SourceType == "Workflow" && !string.IsNullOrWhiteSpace(task.WorkflowContent))
            {
                var steps = ParseWorkflowSteps(task.WorkflowContent);
                if (steps.Count == 0)
                {
                    throw new InvalidOperationException("工作流任务未配置有效步骤");
                }

                var wf = await RunWorkflowAsync(task, steps, recorder,
                    BuildStepAgentResolver(task, agent, logId, "NonInteractive"),
                    null, interactive: false, logId, CancellationToken.None);
                response = BuildWorkflowAnswer(wf);
                if (!wf.Success)
                {
                    workflowFailure = wf.Error ?? "工作流存在失败步骤";
                }
            }
            else
            {
                response = await agent.ProcessMessageAsync(task.PromptContent, new List<string>(), recorder?.Tap);
            }

            var (stepCount, artifactCount, approvalState) = recorder is not null
                ? await recorder.FinishAsync()
                : (StepCount: 0, ArtifactCount: 0, ApprovalState: (string?)null);

            await _traceStore.CompleteLogAsync(logId, workflowFailure is null ? "Success" : "Failed",
                response, workflowFailure, stepCount, artifactCount, approvalState);

            if (workflowFailure is not null)
            {
                Logger.LogWarning("定时工作流执行失败: {TaskName} (Id={TaskId}): {Error}", taskName, taskId, workflowFailure);
                await RescheduleAfterFailureAsync(task);
                return;
            }

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

            // 回写失败日志
            try
            {
                var failCounts = recorder is not null
                    ? await recorder.FinishAsync()
                    : (StepCount: 0, ArtifactCount: 0, ApprovalState: (string?)null);

                await _traceStore.CompleteLogAsync(logId, "Failed", null, ex.Message,
                    failCounts.StepCount, failCounts.ArtifactCount, failCounts.ApprovalState);
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
    /// 工作流引擎：步骤状态机 + 多员工接力 + 人工卡点 + 失败策略。
    /// 每步以指派的员工实例执行（空=任务默认员工），全部前序结果注入后续提示词；
    /// 步骤边界与卡点审批经 forward 实时转发（Channel 桥到 SSE）。
    /// </summary>
    private async Task<WorkflowRunSummaryDto> RunWorkflowAsync(
        TaskEntity task,
        List<WorkflowStepDto> steps,
        ExecutionTraceRecorder? recorder,
        Func<string?, Task<IAgentInstance?>> resolveAgent,
        Func<string, Task>? forward,
        bool interactive,
        Guid logId,
        CancellationToken ct)
    {
        var summary = new WorkflowRunSummaryDto();
        var completed = new List<(int Index, string Name, string Result)>();
        var userId = CurrentUser.Id?.ToString();

        for (var i = 0; i < steps.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var step = steps[i];
            var stepName = string.IsNullOrWhiteSpace(step.Name) ? $"步骤 {i + 1}" : step.Name.Trim();

            if (recorder is not null)
            {
                await recorder.BeginStepAsync(i, stepName);
            }
            await ForwardAsync(forward, SerializeStepEvent("step_start", i, stepName, step.AgentType, null, null));

            string state = "Success";
            string? error = null;
            string? result = null;

            // 人工卡点：交互执行暂停等裁决（复用审批门通道与前端卡片）；定时执行自动通过
            if (step.RequireApproval && error is null && interactive && _toolOptions.Approval.Enabled)
            {
                var gateOutcome = await RequestStepGateAsync(task, logId, i, stepName, step.Prompt, userId, forward);
                if (gateOutcome != ApprovalOutcome.Approved)
                {
                    // 卡点被拒/超时=人工评审不通过，按失败处理（OnFailure=Skip 才可继续）
                    state = "Failed";
                    error = $"人工卡点未通过（{gateOutcome}）";
                    await FinishStepAsync(recorder, forward, i, stepName, state, error, null);
                    AddStepResult(summary, i, stepName, step.AgentType, state, null, error, completed);
                    if (state == "Failed" && step.OnFailure != "Skip")
                    {
                        return CompleteSummary(summary, error);
                    }
                    continue;
                }
            }

            if (error is null)
            {
                try
                {
                    var agent = await resolveAgent(step.AgentType)
                        ?? throw new InvalidOperationException($"步骤「{stepName}」指派员工 {step.AgentType} 不可用");
                    var stepPrompt = BuildStepPrompt(step, i, stepName, completed);
                    result = await agent.ProcessMessageAsync(stepPrompt, new List<string>(), recorder?.Tap);

                    if (result.StartsWith("执行出错:"))
                    {
                        error = result["执行出错:".Length..].Trim();
                        state = "Failed";
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    state = "Failed";
                }
            }

            await FinishStepAsync(recorder, forward, i, stepName, state, error, result);
            AddStepResult(summary, i, stepName, step.AgentType, state, result, error, completed);

            if (state != "Success" && step.OnFailure != "Skip")
            {
                return CompleteSummary(summary, $"步骤 {i + 1}「{stepName}」失败：{TruncateText(error, 200)}");
            }
        }

        summary.Success = summary.FailedSteps == 0;
        return summary;
    }

    private static List<WorkflowStepDto> ParseWorkflowSteps(string json)
    {
        try
        {
            // Web 编辑器存 camelCase、桌面端存 PascalCase——大小写不敏感统一兼容
            return JsonSerializer.Deserialize<List<WorkflowStepDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static WorkflowRunSummaryDto CompleteSummary(WorkflowRunSummaryDto summary, string error)
    {
        summary.Success = false;
        summary.Error = error;
        return summary;
    }

    private async Task<ApprovalOutcome> RequestStepGateAsync(
        TaskEntity task, Guid logId, int stepIndex, string stepName, string stepPrompt,
        string? userId, Func<string, Task>? forward)
    {
        var toolName = $"人工卡点·{stepName}";
        var arguments = TruncateText(stepPrompt, 500);
        var timeout = _toolOptions.Approval.TimeoutSeconds;
        var approvalId = _approvalGateway.Register(
            new ApprovalGateway.PendingRequest(task.Id, logId, toolName, null, arguments, stepIndex, userId),
            timeout);

        await ForwardAsync(forward, JsonSerializer.Serialize(new
        {
            type = "approval_required",
            approvalId,
            toolName,
            skillName = (string?)null,
            toolCallId = "",
            arguments,
            timeoutSeconds = timeout,
            iteration = stepIndex
        }, StreamJsonOptions));

        var result = await _approvalGateway.WaitAsync(approvalId);

        await ForwardAsync(forward, JsonSerializer.Serialize(new
        {
            type = "approval_resolved",
            approvalId,
            toolName,
            decision = result.Outcome.ToString(),
            approverId = result.ApproverId,
            waitMs = result.WaitMs,
            iteration = stepIndex
        }, StreamJsonOptions));

        return result.Outcome;
    }

    private Func<string?, Task<IAgentInstance?>> BuildStepAgentResolver(
        TaskEntity task, IAgentInstance defaultAgent, Guid logId, string mode)
    {
        var cache = new Dictionary<string, IAgentInstance?>(StringComparer.OrdinalIgnoreCase);
        return async t =>
        {
            if (string.IsNullOrWhiteSpace(t) || t == task.AgentType) return defaultAgent;
            if (cache.TryGetValue(t, out var cached)) return cached;
            var instance = await _agentFactory.CreateAgentAsync(t, null,
                new AgentRunContext(task.ProjectId, null, mode, task.Id, logId, CurrentUser.Id?.ToString()));
            cache[t] = instance;
            return instance;
        };
    }

    private static async Task ForwardAsync(Func<string, Task>? forward, string json)
    {
        if (forward is null) return;
        try
        {
            await forward(json);
        }
        catch (OperationCanceledException)
        {
            // 客户端断开：转发通道失效，引擎继续把轨迹写库
        }
    }

    private static async Task FinishStepAsync(
        ExecutionTraceRecorder? recorder, Func<string, Task>? forward,
        int stepIndex, string stepName, string state, string? error, string? result)
    {
        if (recorder is not null)
        {
            await recorder.EndStepAsync(state, error);
        }
        await ForwardAsync(forward, SerializeStepEvent("step_end", stepIndex, stepName, null, state, TruncateText(result ?? error, 200)));
    }

    private static void AddStepResult(
        WorkflowRunSummaryDto summary, int index, string stepName, string? agentType, string state,
        string? result, string? error, List<(int Index, string Name, string Result)> completed)
    {
        summary.Steps.Add(new WorkflowStepResultDto
        {
            Index = index,
            Name = stepName,
            AgentType = agentType,
            State = state,
            Preview = TruncateText(result, 400),
            Error = error is null ? null : TruncateText(error, 200)
        });

        switch (state)
        {
            case "Success":
                summary.CompletedSteps++;
                summary.FinalOutput = result;
                if (result is not null) completed.Add((index, stepName, result));
                break;
            case "Skipped":
                summary.SkippedSteps++;
                break;
            default:
                summary.FailedSteps++;
                break;
        }
    }

    private static string BuildStepPrompt(
        WorkflowStepDto step, int index, string stepName, List<(int Index, string Name, string Result)> completed)
    {
        var body = string.IsNullOrWhiteSpace(step.Prompt) ? stepName : step.Prompt.Trim();
        if (completed.Count == 0) return body;

        var sb = new StringBuilder("已完成步骤的结果如下（请基于它们继续，勿重复已完成的工作）：\n");
        foreach (var c in completed)
        {
            sb.Append($"### 步骤{c.Index + 1}·{c.Name}\n").Append(TruncateText(c.Result, 4000)).Append("\n\n");
        }
        sb.Append("---\n").Append($"当前步骤「{stepName}」任务：").Append(body);
        return sb.ToString();
    }

    private static string BuildWorkflowAnswer(WorkflowRunSummaryDto s)
    {
        var head = s.Error is not null ? $"⛔ 工作流中止：{s.Error}"
            : s.Success ? "✅ 工作流执行成功"
            : $"⚠️ 工作流结束（成功 {s.CompletedSteps} / 失败 {s.FailedSteps} / 跳过 {s.SkippedSteps}）";

        var sb = new StringBuilder(head).AppendLine().AppendLine();
        sb.AppendLine("| 步骤 | 状态 | 摘要 |");
        sb.AppendLine("|---|---|---|");
        foreach (var st in s.Steps)
        {
            var brief = EscapeTableCell(SingleLine(st.Error ?? st.Preview));
            sb.AppendLine($"| {st.Index + 1}·{EscapeTableCell(st.Name)} | {st.State} | {brief} |");
        }

        if (s.Success && !string.IsNullOrWhiteSpace(s.FinalOutput))
        {
            sb.AppendLine().AppendLine("### 最终步骤输出").AppendLine(s.FinalOutput);
        }

        return TruncateText(sb.ToString(), 7800)!;
    }

    private static string EscapeTableCell(string? text) =>
        (text ?? string.Empty).Replace("|", "\\|");

    private static string SingleLine(string? text) =>
        (text ?? string.Empty).Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static string? TruncateText(string? text, int max) =>
        text is null || text.Length <= max ? text : text[..max] + "…";

    private static string SerializeStepEvent(string type, int stepIndex, string? stepName, string? agentType, string? state, string? preview) =>
        JsonSerializer.Serialize(new { type, stepIndex, stepName, agentType, state, preview }, StreamJsonOptions);

    private static readonly JsonSerializerOptions StreamJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
