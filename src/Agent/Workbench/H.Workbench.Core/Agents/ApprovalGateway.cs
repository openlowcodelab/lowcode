using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace H.Workbench.Core.Agents;

public enum ApprovalOutcome
{
    Approved,
    DeniedByUser,
    Timeout,
    Cancelled,
    /// <summary>非交互路径按策略跳过</summary>
    SkippedNonInteractive,
    /// <summary>找不到该审批（已过期/已被裁决/宿主重启）</summary>
    Unknown
}

/// <summary>审批裁决结果（等待时长毫秒；ApproverId 为裁决用户；GrantForRun=本次运行内该工具后续免批）</summary>
public sealed record ApprovalResult(ApprovalOutcome Outcome, string? ApproverId, long WaitMs, bool GrantForRun = false);

/// <summary>
/// 审批门上下文。Rules 为空时行为与阶段B一致（只看技能级 RequiresApproval）。
/// </summary>
public sealed record AgentApprovalContext(
    ApprovalGateway Gateway,
    IReadOnlySet<string> ApprovalTools,
    // None=不启用审批（Chat 路径）；Interactive=可 SSE 回传裁决；NonInteractive=定时/立即执行，按策略直接裁决
    string Mode,
    int TimeoutSeconds,
    int MaxPerExecution,
    bool NonInteractiveAllow,
    Guid TaskId,
    Guid TaskLogId,
    string? UserId,
    ApprovalRuleEvaluator? Rules = null)
{
    /// <summary>本次运行内的预授权工具集（用户点"本次都允许"后，同名工具不再打扰）</summary>
    public HashSet<string> RunGrants { get; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// 工具审批门（进程内）：ReactAgent 执行需人工批准的工具前登记请求并挂起，
/// 前端经 resume API 回传裁决。单实例宿主成立；水平扩展需换成消息总线（阶段B不做）。
/// </summary>
public sealed class ApprovalGateway : IDisposable
{
    public sealed record PendingRequest(
        Guid TaskId,
        Guid TaskLogId,
        string ToolName,
        string? SkillName,
        string? Arguments,
        int Iteration,
        string? UserId);

    private sealed class Pending
    {
        public required PendingRequest Request { get; init; }
        public required TaskCompletionSource<ApprovalResult> Tcs { get; init; }
        public required CancellationTokenSource TimeoutCts { get; init; }
        public required DateTime StartedAt { get; init; }
    }

    private readonly ConcurrentDictionary<Guid, Pending> _pending = new();
    private readonly ILogger<ApprovalGateway> _logger;
    private readonly IDisposable? _stoppingRegistration;

    public ApprovalGateway(ILogger<ApprovalGateway> logger, IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _stoppingRegistration = lifetime.ApplicationStopping.Register(CompleteAllWithCancelled);
    }

    /// <summary>
    /// 该审批是否仍有执行体在等。日志行可能因 Kestrel 检测不到响应侧断连而长期停在 Running，
    /// 所以"能不能裁决"只能问网关本身，不能看日志状态。
    /// </summary>
    public bool IsWaiting(Guid approvalId) => _pending.ContainsKey(approvalId);

    /// <summary>
    /// 登记审批请求，返回 approvalId（不可枚举 GUID，随事件下发给前端）
    /// </summary>
    public Guid Register(PendingRequest request, int timeoutSeconds)
    {
        var approvalId = Guid.NewGuid();
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 5, 3600)));
        var tcs = new TaskCompletionSource<ApprovalResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var startedAt = DateTime.Now;

        var pending = new Pending { Request = request, Tcs = tcs, TimeoutCts = cts, StartedAt = startedAt };
        _pending[approvalId] = pending;

        cts.Token.Register(() =>
        {
            if (_pending.TryRemove(approvalId, out _))
            {
                tcs.TrySetResult(new ApprovalResult(ApprovalOutcome.Timeout, null, (long)(DateTime.Now - startedAt).TotalMilliseconds));
                cts.Dispose();
            }
        });

        _logger.LogInformation("等待人工审批: {ToolName} (approvalId={ApprovalId}, {Timeout}s 内未裁决自动拒绝)",
            request.ToolName, approvalId, timeoutSeconds);
        return approvalId;
    }

    /// <summary>
    /// 等待裁决；调用方取消（如用户关闭页面）时按取消处理并清理登记
    /// </summary>
    public async Task<ApprovalResult> WaitAsync(Guid approvalId, CancellationToken callerCt = default)
    {
        if (!_pending.TryGetValue(approvalId, out var pending))
        {
            return new ApprovalResult(ApprovalOutcome.Unknown, null, 0);
        }

        using (callerCt.Register(() => TryComplete(approvalId, approved: false, userId: "caller-abort", out _)))
        {
            var result = await pending.Tcs.Task;

            // 超时触发后 TCS 完成但调用方仍持有旧引用；统一从字典移除
            _pending.TryRemove(approvalId, out _);
            return result;
        }
    }

    /// <summary>
    /// 回传裁决。未知/已过期 id 返回 false（幂等：重复裁决第二次失败）；
    /// userId 与登记时不一致返回 false（防跨用户猜 id）。
    /// </summary>
    public bool TryComplete(Guid approvalId, bool approved, string? userId, out ApprovalResult result, bool grantForRun = false)
    {
        if (_pending.TryRemove(approvalId, out var pending))
        {
            if (userId != pending.Request.UserId)
            {
                // 归属不符：把请求放回去也不安全，直接按拒绝终止等待
                var denied = new ApprovalResult(ApprovalOutcome.DeniedByUser, userId, Elapsed(pending));
                pending.Tcs.TrySetResult(denied);
                DisposeCts(pending);
                _logger.LogWarning("审批 {ApprovalId} 裁决用户与发起用户不符，按拒绝处理", approvalId);
                result = denied;
                return false;
            }

            result = new ApprovalResult(
                approved ? ApprovalOutcome.Approved : ApprovalOutcome.DeniedByUser,
                userId,
                Elapsed(pending),
                approved && grantForRun);
            pending.Tcs.TrySetResult(result);
            DisposeCts(pending);
            _logger.LogInformation("审批 {ApprovalId} 已裁决: {Outcome}（by {UserId}）",
                approvalId, result.Outcome, userId ?? "(anonymous)");
            return true;
        }

        result = new ApprovalResult(ApprovalOutcome.Unknown, null, 0);
        return false;
    }

    private static long Elapsed(Pending pending) => (long)(DateTime.Now - pending.StartedAt).TotalMilliseconds;

    private static void DisposeCts(Pending pending)
    {
        try { pending.TimeoutCts.Dispose(); } catch { /* 已触发/已释放 */ }
    }

    private void CompleteAllWithCancelled()
    {
        foreach (var id in _pending.Keys)
        {
            if (_pending.TryRemove(id, out var pending))
            {
                pending.Tcs.TrySetResult(new ApprovalResult(ApprovalOutcome.Cancelled, null, Elapsed(pending)));
                DisposeCts(pending);
            }
        }
    }

    public void Dispose() => _stoppingRegistration?.Dispose();
}
