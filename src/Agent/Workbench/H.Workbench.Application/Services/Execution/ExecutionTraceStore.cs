using H.Workbench.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace H.Workbench.Application.Services.Execution;

/// <summary>
/// 轨迹/产物写入器。
/// 每次调用独立短 UoW（requiresNew）+ CompleteAsync 立即释放连接——
/// 流式执行/审批挂起的长 UoW 内不得承载这些写入，否则长事务占用连接。
/// 红线：工具（Core 单例）零持久化依赖，本类只消费 AppService 层转交的事件数据。
/// </summary>
public class ExecutionTraceStore : ITransientDependency
{
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IRepository<TaskExecutionStepEntity, Guid> _stepRepository;
    private readonly IRepository<ArtifactEntity, Guid> _artifactRepository;
    private readonly IRepository<TaskLogEntity, Guid> _logRepository;
    private readonly ILogger<ExecutionTraceStore> _logger;

    public ExecutionTraceStore(
        IUnitOfWorkManager unitOfWorkManager,
        IRepository<TaskExecutionStepEntity, Guid> stepRepository,
        IRepository<ArtifactEntity, Guid> artifactRepository,
        IRepository<TaskLogEntity, Guid> logRepository,
        ILogger<ExecutionTraceStore> logger)
    {
        _unitOfWorkManager = unitOfWorkManager;
        _stepRepository = stepRepository;
        _artifactRepository = artifactRepository;
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task AppendStepsAsync(IEnumerable<TaskExecutionStepEntity> steps)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        foreach (var step in steps)
        {
            await _stepRepository.InsertAsync(step, autoSave: true);
        }
        await uow.CompleteAsync();
    }

    /// <summary>
    /// 工作流步骤头部行状态回填（执行中→Success/Failed/Skipped）
    /// </summary>
    public async Task UpdateStepOutcomeAsync(Guid stepId, string? content, bool isError)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        var step = await _stepRepository.FindAsync(stepId);
        if (step != null)
        {
            step.Content = content;
            step.IsError = isError;
            await _stepRepository.UpdateAsync(step, autoSave: true);
        }
        await uow.CompleteAsync();
    }

    public async Task AddArtifactsAsync(IEnumerable<ArtifactEntity> artifacts)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        foreach (var artifact in artifacts)
        {
            await _artifactRepository.InsertAsync(artifact, autoSave: true);
        }
        await uow.CompleteAsync();
    }

    /// <summary>
    /// 按 approvalId 回填裁决结果。审批中心与对话页都不依赖执行侧的内存簿记，
    /// 裁决一旦生效就立刻把行状态写死——否则刷新后同一请求还会显示"待裁决"。
    /// </summary>
    public async Task UpdateApprovalByIdAsync(Guid approvalId, string state, string? approverId, int? waitMs)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        var step = await _stepRepository.FindAsync(s => s.Kind == "Approval" && s.ApprovalId == approvalId);
        if (step != null)
        {
            step.ApprovalState = state;
            if (approverId is not null) step.ApproverId = approverId;
            if (waitMs is not null) step.WaitMs = waitMs;
            await _stepRepository.UpdateAsync(step, autoSave: true);
        }
        else
        {
            _logger.LogWarning("未找到 approvalId={ApprovalId} 对应的审批轨迹行，裁决结果无法回填", approvalId);
        }
        await uow.CompleteAsync();
    }

    /// <summary>
    /// 立 Running 行。必须走独立短 UoW 并立即提交：外层若为事务性 UoW（ExecuteNowAsync 等
    /// 经 ABP 拦截器包裹的路径），ambient 内插入要等请求结束才可见，后续 requiresNew 收尾会查不到行。
    /// </summary>
    public async Task StartLogAsync(Guid logId, Guid taskId, string? prompt, DateTime startTime)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        await _logRepository.InsertAsync(new TaskLogEntity(logId)
        {
            TaskId = taskId,
            Prompt = prompt,
            StartTime = startTime,
            Status = "Running"
        });
        await uow.CompleteAsync();
    }

    public async Task CompleteLogAsync(
        Guid logId,
        string status,
        string? result,
        string? errorMessage,
        int stepCount,
        int artifactCount,
        string? approvalState,
        int promptTokens = 0,
        int completionTokens = 0,
        string? verdict = null,
        string? verdictReason = null)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        var log = await _logRepository.FindAsync(logId);
        if (log != null)
        {
            log.Status = status;
            log.Result = result;
            log.ErrorMessage = errorMessage;
            log.StepCount = stepCount;
            log.ArtifactCount = artifactCount;
            log.ApprovalState = approvalState;
            log.PromptTokens = promptTokens;
            log.CompletionTokens = completionTokens;
            log.Verdict = verdict;
            log.VerdictReason = verdictReason;
            log.EndTime = DateTime.Now;
            await _logRepository.UpdateAsync(log);
        }
        else
        {
            // Running 行丢失（如被清扫器抢先处理）只可能出现在异常场景，留痕即可
            _logger.LogWarning("TaskLog {LogId} 不存在，执行结果无法回写（步骤 {Steps}、产物 {Artifacts} 已独立落库）",
                logId, stepCount, artifactCount);
        }

        // ABP 的 UoW 只在 CompleteAsync 时提交，任何分支都不得提前 return 跳过
        await uow.CompleteAsync();
    }
}
