using H.Workbench.Core;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace H.Workbench.Application.Services.Execution;

/// <summary>
/// 宿主内并发执行配额。无人值守执行一旦失控（坏 cron、验收不过反复重跑）会同时压满
/// LLM 配额与 git 工作区，这里给一个进程内硬上限。多实例部署时上限按实例计。
/// </summary>
public class RunQuotaGate : ITransientDependency
{
    private readonly SemaphoreSlim _slots;
    private readonly int _max;

    public RunQuotaGate(IOptions<WorkbenchToolOptions> options)
    {
        _max = Math.Max(1, options.Value.Budget.MaxConcurrentRuns);
        _slots = new SemaphoreSlim(_max);
    }

    public int MaxConcurrentRuns => _max;

    /// <summary>
    /// 立即占位，不排队。返回 null 表示已达上限——调用方应明确拒绝而非静默等待，
    /// 否则用户看到的是"任务卡住"而不是"当前并发已满"。
    /// </summary>
    public IDisposable? TryAcquire() => _slots.Wait(0) ? new Slot(this) : null;

    private sealed class Slot(RunQuotaGate gate) : IDisposable
    {
        public void Dispose() => gate._slots.Release();
    }
}
