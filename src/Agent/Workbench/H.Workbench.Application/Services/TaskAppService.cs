using AutoMapper;
using H.Abp.Application.Contracts;
using H.Workbench.Application.Contracts;
using H.Workbench.Application.Services.Execution;
using H.Workbench.Application.Workers;
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
    private readonly WorkbenchVerifier _verifier;
    private readonly RunQuotaGate _runQuota;
    private readonly WorkbenchRunQueue _runQueue;
    private readonly RunEventHub _runHub;

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
        IOptions<WorkbenchToolOptions> toolOptions,
        WorkbenchVerifier verifier,
        RunQuotaGate runQuota,
        WorkbenchRunQueue runQueue,
        RunEventHub runHub)
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
        _verifier = verifier;
        _runQuota = runQuota;
        _runQueue = runQueue;
        _runHub = runHub;
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
            AcceptanceCriteria = string.IsNullOrWhiteSpace(input.AcceptanceCriteria) ? null : input.AcceptanceCriteria.Trim(),
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
        task.AcceptanceCriteria = string.IsNullOrWhiteSpace(input.AcceptanceCriteria) ? null : input.AcceptanceCriteria.Trim();
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

    /// <summary>
    /// 兼容既有前端的“提交即订阅”：把运行交给宿主，然后订阅它的事件流。
    /// 与阶段B的本质差别——这条连接断开不再取消执行。
    /// </summary>
    public async IAsyncEnumerable<string> ExecuteStreamAsync(ExecuteTaskStreamInputDto input)
    {
        var started = await StartRunAsync(new StartRunInputDto
        {
            TaskId = input.TaskId,
            Prompt = input.Prompt
        });

        if (!started.Success || started.Data == Guid.Empty)
        {
            yield return SerializeError(started.Message ?? "任务无法启动");
            yield break;
        }

        await foreach (var evt in _runHub.SubscribeAsync(started.Data))
        {
            yield return evt;
        }
    }

    public async Task<BaseOutput<Guid>> StartRunAsync(StartRunInputDto input)
    {
        var task = await _taskRepository.FindAsync(input.TaskId);
        if (task == null)
        {
            return new BaseOutput<Guid> { Code = 1, Success = false, Message = $"任务不存在: {input.TaskId}" };
        }

        var runId = Guid.NewGuid();
        _runHub.Open(runId);
        _runQueue.Enqueue(new RunRequest(task.Id, runId, input.Prompt, CurrentUser.Id?.ToString()));
        return new(runId);
    }

    public Task<BaseOutput> CancelRunAsync(Guid runId) =>
        Task.FromResult(_runQueue.TryCancel(runId)
            ? new BaseOutput()
            : new BaseOutput { Code = 1, Success = false, Message = "该运行已结束或不存在" });

    public async Task<BaseOutput<RunStatusDto>> GetRunStatusAsync(Guid runId)
    {
        var log = await _logRepository.FindAsync(runId);
        return new(new RunStatusDto
        {
            RunId = runId,
            Status = log?.Status ?? "NotFound",
            Tracked = _runHub.IsTracked(runId),
            StepCount = log?.StepCount ?? 0,
            Verdict = log?.Verdict
        });
    }

    public IAsyncEnumerable<string> SubscribeRunAsync(Guid runId, CancellationToken ct = default) =>
        _runHub.SubscribeAsync(runId, ct);

    /// <summary>
    /// 一次运行的结果。SkippedByQuota 让调用方（定时 Worker）能区分"没跑成"和"根本没起跑"。
    /// </summary>
    private sealed record RunOutcome(string Status, bool Succeeded, string? Error, bool SkippedByQuota = false);

    public Task ExecuteDetachedAsync(Guid taskId, Guid runId, string? promptOverride, CancellationToken ct) =>
        RunCoreAsync(taskId, runId, promptOverride, "Interactive",
            json => { _runHub.Publish(runId, json); return Task.CompletedTask; }, ct);

    /// <summary>
    /// 执行内核：一次运行的完整生命周期（Running 行 → ReAct/工作流 → 验收 → 收尾落库）。
    /// 事件先落轨迹再交给 sink（sink 为空表示无人收听，如定时执行）；
    /// ct 来自宿主的运行令牌（显式取消或宿主关闭），不再是浏览器连接。
    /// </summary>
    private async Task<RunOutcome> RunCoreAsync(
        Guid taskId, Guid runId, string? promptOverride, string approvalMode,
        Func<string, Task>? sink, CancellationToken ct)
    {
        var task = await _taskRepository.FindAsync(taskId);
        if (task == null)
        {
            const string msg = "任务不存在";
            sink?.Invoke(SerializeError($"{msg}: {taskId}"));
            return new("Failed", false, $"{msg}: {taskId}");
        }

        var slot = _runQuota.TryAcquire();
        if (slot is null)
        {
            var quotaMsg = $"并发执行已达上限（{_runQuota.MaxConcurrentRuns}），请稍后重试";
            sink?.Invoke(SerializeError(quotaMsg));
            return new("Failed", false, quotaMsg, SkippedByQuota: true);
        }

        var prompt = string.IsNullOrWhiteSpace(promptOverride) ? task.PromptContent : promptOverride.Trim();
        var userId = _runQueue.PeekUser(runId) ?? CurrentUser.Id?.ToString();
        var startTime = DateTime.Now;
        var thinking = new StringBuilder();
        var answer = new StringBuilder();
        string? failure = null;
        VerificationOutcome? verdict = null;

        // 续聊上下文只取 Status=Success 行，且必须在 Running 行插入前构建
        var history = await BuildConversationHistoryAsync(task);

        try
        {
            await _traceStore.StartLogAsync(runId, task.Id, prompt, startTime);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "插入 Running 执行日志失败 TaskId={TaskId}", task.Id);
        }

        var isWorkflow = task.SourceType == "Workflow" && !string.IsNullOrWhiteSpace(task.WorkflowContent);
        var steps = isWorkflow ? ParseWorkflowSteps(task.WorkflowContent!) : null;

        IAgentInstance? agent = null;
        try
        {
            agent = await _agentFactory.CreateAgentAsync(task.AgentType, task.ModelConfigId,
                new AgentRunContext(task.ProjectId, null, approvalMode, task.Id, runId, userId));
        }
        catch (Exception ex)
        {
            failure = ex.Message;
        }

        if (agent is null)
        {
            failure ??= $"无法创建员工实例: {task.AgentType}";
            await _traceStore.CompleteLogAsync(runId, "Failed", null, failure, 0, 0, null);
            sink?.Invoke(SerializeError(failure));
            slot.Dispose();
            return new("Failed", false, failure);
        }

        var recorder = new ExecutionTraceRecorder(_traceStore, _toolOptions.Trace, Logger, runId, task.Id);
        var completedNormally = false;
        var outcomeStatus = "Failed";
        var outcomeSucceeded = false;
        string? outcomeError = null;

        // 一份事件两个去处：先落轨迹再交给订阅者，两者都不能抛出去打断执行
        async Task Forward(string json)
        {
            await recorder.HandleAsync(json);
            if (sink is not null) await sink(json);
        }

        try
        {
            if (isWorkflow)
            {
                try
                {
                    var summary = await RunWorkflowAsync(task, steps!, recorder,
                        BuildStepAgentResolver(task, agent, runId, approvalMode),
                        Forward, interactive: approvalMode == "Interactive", runId, ct);

                    var md = BuildWorkflowAnswer(summary);
                    answer.Append(md);
                    if (!summary.Success)
                    {
                        failure = summary.Error ?? "工作流存在失败步骤";
                    }
                    await Forward(SerializeAnswer(md));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    failure = ex.Message;
                }
            }
            else if (agent is IStreamingAgent streamingAgent)
            {
                await foreach (var chunk in streamingAgent.ProcessMessageStreamAsync(prompt, history, ct))
                {
                    AccumulateStreamEvent(chunk, thinking, answer, ref failure);
                    await Forward(chunk);
                }
            }
            else
            {
                var response = await agent.ProcessMessageAsync(prompt, new List<string>(), recorder.Tap);
                answer.Append(response);
                await Forward(SerializeAnswer(response));
            }

            if (failure is not null && !ct.IsCancellationRequested)
            {
                await Forward(SerializeError(failure));
            }

            // 验收：员工声称完成之后，按任务登记的验收标准复核再定成败
            var candidateAnswer = answer.Length > 0 ? answer.ToString() : thinking.ToString();
            if (failure is null && candidateAnswer.Length > 0 && !ct.IsCancellationRequested)
            {
                verdict = await VerifyResultAsync(task, prompt, runId, candidateAnswer);
                if (verdict is not null)
                {
                    await Forward(SerializeVerifyEvent(verdict));
                }
            }

            completedNormally = true;
        }
        catch (OperationCanceledException)
        {
            // 显式取消/宿主关闭：不算失败，收尾按 Cancelled 记
        }
        finally
        {
            var finalAnswer = answer.Length > 0 ? answer.ToString() : thinking.ToString();
            var aborted = !completedNormally || ct.IsCancellationRequested;
            var succeeded = !aborted && failure is null && finalAnswer.Length > 0;

            try
            {
                var (stepCount, artifactCount, approvalState) = await recorder.FinishAsync();
                var verified = verdict is null || verdict.Verdict != "Fail";

                outcomeStatus = aborted ? "Cancelled" : succeeded && verified ? "Success" : "Failed";
                outcomeSucceeded = outcomeStatus == "Success";
                outcomeError = aborted ? "运行被取消（用户停止或宿主关闭）"
                    : succeeded && !verified ? $"验收未通过：{verdict!.Reason}"
                    : failure;

                await _traceStore.CompleteLogAsync(
                    runId,
                    outcomeStatus,
                    // 验收未通过也要留下员工实际产出的文本：裁决是评价，不是删除证据
                    succeeded ? finalAnswer : null,
                    outcomeError,
                    stepCount,
                    artifactCount,
                    approvalState,
                    recorder.PromptTokens + (verdict?.PromptTokens ?? 0),
                    recorder.CompletionTokens + (verdict?.CompletionTokens ?? 0),
                    verdict?.Verdict,
                    verdict?.Reason);

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
                Logger.LogError(ex, "运行结果落库失败 TaskId={TaskId} RunId={RunId}", task.Id, runId);
            }

            slot.Dispose();
        }

        return new(outcomeStatus, outcomeSucceeded, outcomeError);
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

    private static string SerializeVerifyEvent(VerificationOutcome verdict)
        => JsonSerializer.Serialize(new { type = "verify", verdict = verdict.Verdict, reason = verdict.Reason, iteration = 0 });

    /// <summary>
    /// 结果验收：只在任务登记了验收标准时进行，证据=本次执行落库的产物（含失败标记）。
    /// 返回 null 表示"未裁决"，调用方不得视为通过。
    /// </summary>
    private async Task<VerificationOutcome?> VerifyResultAsync(
        TaskEntity task, string prompt, Guid logId, string result)
    {
        if (!_toolOptions.Verification.Enabled || string.IsNullOrWhiteSpace(task.AcceptanceCriteria))
        {
            return null;
        }

        var evidence = new List<string>();
        try
        {
            var queryable = await _artifactRepository.GetQueryableAsync();
            evidence = await AsyncExecuter.ToListAsync(
                queryable.Where(a => a.TaskLogId == logId)
                    .OrderBy(a => a.CreationTime)
                    .Select(a => a.Success ? a.Title : a.Title + "（失败）"));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "验收证据收集失败 TaskLogId={LogId}", logId);
        }

        return await _verifier.VerifyAsync(task.AcceptanceCriteria, prompt, result, evidence);
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
    /// 人工验收：把一次执行产出的全部产物整体标记为认可/不认可。
    /// 提交已推远端，"不认可"撤销不了事实，但它让成果有了可追溯的判断，
    /// 而不是只留下一行没人消费的 CommitHash。
    /// </summary>
    public async Task<BaseOutput<int>> ReviewArtifactsAsync(ReviewArtifactsInputDto input)
    {
        var status = input.Status switch
        {
            "Accepted" or "Rejected" or "Pending" => input.Status,
            _ => throw new Volo.Abp.Validation.AbpValidationException($"未知验收状态: {input.Status}")
        };

        var queryable = await _artifactRepository.GetQueryableAsync();
        var artifacts = await AsyncExecuter.ToListAsync(queryable.Where(a => a.TaskLogId == input.TaskLogId));
        if (artifacts.Count == 0) return new(0);

        var reviewerId = CurrentUser.Id?.ToString();
        var now = DateTime.Now;
        var note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();

        foreach (var artifact in artifacts)
        {
            artifact.ReviewStatus = status;
            artifact.ReviewNote = status == "Pending" ? null : note;
            artifact.ReviewerId = status == "Pending" ? null : reviewerId;
            artifact.ReviewedAt = status == "Pending" ? null : now;
            await _artifactRepository.UpdateAsync(artifact, autoSave: true);
        }

        return new(artifacts.Count);
    }

    /// <summary>
    /// 跨任务审批队列：审批散落在各对话页的内联卡里时，用户根本不知道有多少在等他。
    /// Live=false 表示所属执行已结束，条目只是留痕、无法再裁决。
    /// </summary>
    public async Task<BaseOutput<List<ApprovalQueueItemDto>>> GetApprovalQueueAsync(string? state = null, int maxCount = 50)
    {
        var wanted = string.IsNullOrWhiteSpace(state) ? "Pending" : state.Trim();

        var stepQueryable = await _stepRepository.GetQueryableAsync();
        var logQueryable = await _logRepository.GetQueryableAsync();
        var taskQueryable = await _taskRepository.GetQueryableAsync();

        var query = stepQueryable
                .Where(s => s.Kind == "Approval" && s.ApprovalState == wanted)
                .Join(logQueryable, s => s.TaskLogId, l => l.Id, (s, l) => new { Step = s, Log = l })
                .Join(taskQueryable, x => x.Log.TaskId, t => t.Id, (x, t) => new { x.Step, x.Log, Task = t });

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.Step.StartedAt).Take(Math.Clamp(maxCount, 1, 200)));

        return new(rows.Select(x => new ApprovalQueueItemDto
        {
            Id = x.Step.Id,
            ApprovalId = x.Step.ApprovalId,
            TaskLogId = x.Step.TaskLogId,
            TaskId = x.Log.TaskId,
            TaskName = x.Task.TaskName,
            AgentType = x.Task.AgentType,
            ToolName = x.Step.ToolName,
            SkillName = x.Step.SkillName,
            Arguments = x.Step.Arguments,
            ApprovalState = x.Step.ApprovalState ?? wanted,
            LogStatus = x.Log.Status,
            // 有执行体真的在等才算可裁决：僵尸 Running 日志里的 Pending 行点不动
            Live = x.Log.Status == "Running" && x.Step.ApprovalId is { } aid && _approvalGateway.IsWaiting(aid),
            StartedAt = x.Step.StartedAt,
            ApproverId = x.Step.ApproverId,
            WaitMs = x.Step.WaitMs
        }).ToList());
    }

    /// <summary>
    /// 运行看板。窗口内的执行量不大（按任务日志行数级），一次取回内存聚合，
    /// 避免为了分组把 SQL 写成不可读的表达式树。
    /// </summary>
    public async Task<BaseOutput<RuntimeStatsDto>> GetRuntimeStatsAsync(int days = 7)
    {
        var window = Math.Clamp(days, 1, 90);
        var from = DateTime.Now.AddDays(-window);

        var logQueryable = await _logRepository.GetQueryableAsync();
        var taskQueryable = await _taskRepository.GetQueryableAsync();

        var rows = await AsyncExecuter.ToListAsync(
            logQueryable.Where(l => l.StartTime >= from)
                .Join(taskQueryable, l => l.TaskId, t => t.Id, (l, t) => new { Log = l, t.TaskName, t.AgentType }));

        var stats = new RuntimeStatsDto
        {
            Days = window,
            From = from,
            TotalRuns = rows.Count,
            SuccessRuns = rows.Count(x => x.Log.Status == "Success"),
            FailedRuns = rows.Count(x => x.Log.Status == "Failed"),
            OtherRuns = rows.Count(x => x.Log.Status != "Success" && x.Log.Status != "Failed"),
            VerdictPass = rows.Count(x => x.Log.Verdict == "Pass"),
            VerdictFail = rows.Count(x => x.Log.Verdict == "Fail"),
            VerdictUnclear = rows.Count(x => x.Log.Verdict == "Unclear"),
            UnverifiedRuns = rows.Count(x => string.IsNullOrEmpty(x.Log.Verdict)),
            PromptTokens = rows.Sum(x => (long)x.Log.PromptTokens),
            CompletionTokens = rows.Sum(x => (long)x.Log.CompletionTokens)
        };

        var judged = stats.SuccessRuns + stats.FailedRuns;
        stats.SuccessRate = judged == 0 ? 0 : Math.Round(stats.SuccessRuns * 100.0 / judged, 1);
        stats.AvgDurationSeconds = Math.Round(
            rows.Where(x => x.Log.EndTime.HasValue).Average(x => (x.Log.EndTime!.Value - x.Log.StartTime).TotalSeconds), 1);

        stats.ByAgent = rows.GroupBy(x => x.AgentType ?? "")
            .Select(g => new AgentRuntimeStat
            {
                AgentType = g.Key,
                Runs = g.Count(),
                Success = g.Count(x => x.Log.Status == "Success"),
                Failed = g.Count(x => x.Log.Status == "Failed"),
                VerdictFail = g.Count(x => x.Log.Verdict == "Fail"),
                Tokens = g.Sum(x => (long)x.Log.PromptTokens + x.Log.CompletionTokens),
                AvgDurationSeconds = Math.Round(
                    g.Where(x => x.Log.EndTime.HasValue)
                     .Select(x => (x.Log.EndTime!.Value - x.Log.StartTime).TotalSeconds)
                     .DefaultIfEmpty(0).Average(), 1)
            })
            .OrderByDescending(a => a.Tokens)
            .ToList();

        stats.TopTasks = rows.GroupBy(x => new { x.Log.TaskId, x.TaskName, x.AgentType })
            .Select(g => new TaskRuntimeStat
            {
                TaskId = g.Key.TaskId,
                TaskName = g.Key.TaskName,
                AgentType = g.Key.AgentType,
                Runs = g.Count(),
                Failed = g.Count(x => x.Log.Status == "Failed"),
                Tokens = g.Sum(x => (long)x.Log.PromptTokens + x.Log.CompletionTokens)
            })
            .OrderByDescending(t => t.Tokens)
            .Take(10)
            .ToList();

        var stepQueryable = await _stepRepository.GetQueryableAsync();
        stats.PendingApprovals = await AsyncExecuter.CountAsync(
            stepQueryable.Where(s => s.Kind == "Approval" && s.ApprovalState == "Pending"));

        var artifactQueryable = await _artifactRepository.GetQueryableAsync();
        stats.PendingArtifactReviews = await AsyncExecuter.CountAsync(
            artifactQueryable.Where(a => a.ReviewStatus == "Pending"));

        return new(stats);
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
        var completed = _approvalGateway.TryComplete(
            input.ApprovalId, input.Approved, userId, out var result, input.GrantForRun);

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

        // 裁决生效即回写轨迹行：不依赖执行侧 recorder 的内存配对，
        // 否则审批中心与刷新后的对话页会一直显示"待裁决"
        await _traceStore.UpdateApprovalByIdAsync(
            input.ApprovalId,
            input.Approved ? "Approved" : "Denied",
            userId,
            (int)Math.Min(result.WaitMs, int.MaxValue));

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

    /// <summary>
    /// 定时/立即执行：跑内核，然后只做调度才需要的收尾（重排下次执行、Once 完成、失败退避）。
    /// 执行逻辑与对话路径共用同一个内核，避免两条路径各自演化出不同行为。
    /// </summary>
    private async Task ExecuteTaskInternalAsync(TaskEntity task)
    {
        Logger.LogInformation("开始执行定时任务 {TaskName} (Id={TaskId})", task.TaskName, task.Id);

        var runId = Guid.NewGuid();
        var outcome = await RunCoreAsync(task.Id, runId, null, "NonInteractive", null, CancellationToken.None);

        if (outcome.SkippedByQuota)
        {
            throw new InvalidOperationException(outcome.Error ?? "并发执行已达上限");
        }

        var fresh = await _taskRepository.FindAsync(task.Id);
        if (fresh is null) return;

        if (!outcome.Succeeded)
        {
            Logger.LogWarning("定时任务未通过验收或执行失败: {TaskName} (Id={TaskId}): {Error}",
                task.TaskName, task.Id, outcome.Error);
            await RescheduleAfterFailureAsync(fresh);
            return;
        }

        // 手动任务不参与调度，保持下次执行时间为空
        fresh.NextExecutionTime = fresh.ExecutionMode == "Manual"
            ? null
            : CalculateNextExecutionTime(
                fresh.ScheduleType, fresh.CronExpression, fresh.Hour, fresh.Minute, fresh.DayOfWeek, fresh.DayOfMonth);

        if (fresh.ExecutionMode == "Auto" && fresh.ScheduleType == "Once")
        {
            fresh.Status = "Completed";
            fresh.IsEnabled = false;
        }

        await _taskRepository.UpdateAsync(fresh);
        Logger.LogInformation("定时任务执行完成: {TaskName} (Id={TaskId})", task.TaskName, task.Id);
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
