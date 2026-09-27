using H.Workbench.Application.Contracts;
using H.Workbench.Application.Services.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace H.Workbench.Application.Workers;

/// <summary>
/// 一次脱离 HTTP 请求的运行请求。
/// UserId 必须在提交时捕获：宿主里没有 HttpContext，ABP 的 CurrentUser 取不到值，
/// 而审批归属校验要用它——不传就会导致"发起者裁决自己被拒"。
/// StartFromStep/CarriedResults 只在工作流续跑时有值：失败之前的步骤不重做。
/// </summary>
public sealed record RunRequest(
    Guid TaskId,
    Guid RunId,
    string? Prompt,
    string? UserId,
    int StartFromStep = 0,
    IReadOnlyDictionary<int, string>? CarriedResults = null);

/// <summary>
/// 运行队列与取消登记（单例）。执行宿主从这里取任务跑。
/// </summary>
public class WorkbenchRunQueue
{
    private readonly Channel<RunRequest> _channel = Channel.CreateUnbounded<RunRequest>(
        new UnboundedChannelOptions { SingleReader = true });

    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellations = new();

    private readonly ConcurrentDictionary<Guid, RunRequest> _requests = new();

    public void Enqueue(RunRequest request)
    {
        _requests[request.RunId] = request;
        _cancellations[request.RunId] = new CancellationTokenSource();
        _channel.Writer.TryWrite(request);
    }

    /// <summary>
    /// 提交时捕获的发起人。审批归属校验要用它，而宿主里没有 HttpContext，
    /// 所以只能在这里（有请求上下文的时刻）取一次。
    /// </summary>
    public string? PeekUser(Guid runId) => _requests.TryGetValue(runId, out var r) ? r.UserId : null;

    /// <summary>续跑计划：非空表示这次运行从第 N 步开始，前序步骤沿用旧结果</summary>
    public (int StartFromStep, IReadOnlyDictionary<int, string>? Carried)? PeekResumePlan(Guid runId) =>
        _requests.TryGetValue(runId, out var r) && r.StartFromStep > 0
            ? (r.StartFromStep, r.CarriedResults)
            : null;

    public bool TryCancel(Guid runId)
    {
        if (_cancellations.TryGetValue(runId, out var cts))
        {
            cts.Cancel();
            return true;
        }
        return false;
    }

    public bool IsRunning(Guid runId) => _cancellations.ContainsKey(runId);

    public CancellationTokenSource? GetCts(Guid runId) =>
        _cancellations.TryGetValue(runId, out var cts) ? cts : null;

    public void Release(Guid runId)
    {
        _requests.TryRemove(runId, out _);
        if (_cancellations.TryRemove(runId, out var cts))
        {
            cts.Dispose();
        }
    }

    public ChannelReader<RunRequest> Reader => _channel.Reader;
}

/// <summary>
/// 执行宿主：在后台把任务跑完，事件推给 RunEventHub，轨迹与结果照常落库。
///
/// 这一步改变的是本模块最根本的一条假设——此前执行体寄生在 SSE 请求里，
/// 浏览器一关执行就被取消（TaskAppService 用 HttpContext.RequestAborted 驱动 ReAct 循环），
/// 所以"数字员工"只能当着人的面干活。现在连接只是视图，断连不影响执行。
/// </summary>
public class WorkbenchRunHost : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly WorkbenchRunQueue _queue;
    private readonly RunEventHub _hub;
    private readonly ILogger<WorkbenchRunHost> _logger;

    public WorkbenchRunHost(
        IServiceProvider serviceProvider,
        WorkbenchRunQueue queue,
        RunEventHub hub,
        ILogger<WorkbenchRunHost> logger)
    {
        _serviceProvider = serviceProvider;
        _queue = queue;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Workbench 执行宿主已启动");

        var trimAt = DateTime.Now.AddMinutes(5);

        await foreach (var request in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            if (DateTime.Now > trimAt)
            {
                _hub.Trim();
                trimAt = DateTime.Now.AddMinutes(5);
            }

            // 只负责派发：一次运行可能持续几十分钟，串行消费会让后面的任务干等。
            // 真正的并发上限由执行内核里的 RunQuotaGate 把关。
            _ = Task.Run(() => ExecuteAsync(request, stoppingToken), CancellationToken.None);
        }
    }

    private async Task ExecuteAsync(RunRequest request, CancellationToken stoppingToken)
    {
        var runCts = _queue.GetCts(request.RunId);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            stoppingToken, runCts?.Token ?? CancellationToken.None);

        _hub.Open(request.RunId);

        try
        {
            // 每个运行独立 scope：执行可能持续几十分钟，不能借用任何请求级上下文
            using var scope = _serviceProvider.CreateScope();
            var taskService = scope.ServiceProvider.GetRequiredService<ITaskAppService>();

            await taskService.ExecuteDetachedAsync(
                request.TaskId, request.RunId, request.Prompt, linked.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "运行 {RunId} 执行异常", request.RunId);
        }
        finally
        {
            _hub.Complete(request.RunId);
            _queue.Release(request.RunId);
        }
    }
}
