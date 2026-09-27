using System.Collections.Concurrent;
using System.Threading.Channels;

namespace H.Workbench.Application.Services.Execution;

/// <summary>
/// 运行事件中枢：执行体把事件推到这里，SSE 连接只是其中一个订阅者。
/// 这是"执行宿主剥离"的关键——浏览器关掉、网络断了，执行体照旧跑完，
/// 回来时按 runId 重新订阅即可看到缓冲里的事件与库里的轨迹。
///
/// 单例、纯内存、不依赖 scoped 服务（红线同 ToolRegistry）。
/// 缓冲有界：只保留最近 BufferLimit 条事件，更早的过程由 TaskExecutionStep 轨迹承担。
/// </summary>
public class RunEventHub
{
    private const int BufferLimit = 4000;

    private sealed class RunState
    {
        public readonly List<string> Buffer = new();
        public readonly List<Channel<string>> Subscribers = new();
        public bool Completed;
        public DateTime CompletedAt = DateTime.MinValue;
    }

    private readonly ConcurrentDictionary<Guid, RunState> _runs = new();
    private readonly TimeSpan _retain = TimeSpan.FromMinutes(10);

    /// <summary>
    /// 开始一个运行的事件通道。重复调用幂等（重连不会清空缓冲）。
    /// </summary>
    public void Open(Guid runId)
    {
        _runs.GetOrAdd(runId, _ => new RunState());
    }

    /// <summary>
    /// 推一条事件：入缓冲 + 广播给所有订阅者。已结束的运行按丢弃处理（执行体已收尾）。
    /// </summary>
    public void Publish(Guid runId, string eventJson)
    {
        if (!_runs.TryGetValue(runId, out var state)) return;

        lock (state)
        {
            if (state.Completed) return;
            state.Buffer.Add(eventJson);
            if (state.Buffer.Count > BufferLimit)
            {
                state.Buffer.RemoveRange(0, state.Buffer.Count - BufferLimit);
            }

            foreach (var sub in state.Subscribers)
            {
                sub.Writer.TryWrite(eventJson);
            }
        }
    }

    public void Complete(Guid runId)
    {
        if (!_runs.TryGetValue(runId, out var state)) return;

        List<Channel<string>> subs;
        lock (state)
        {
            if (state.Completed) return;
            state.Completed = true;
            state.CompletedAt = DateTime.Now;
            subs = state.Subscribers.ToList();
        }

        foreach (var sub in subs)
        {
            sub.Writer.TryComplete();
        }
    }

    /// <summary>
    /// 订阅某个运行：先回放缓冲（含已缓冲的历史事件），再接实时流。
    /// 返回的枚举在运行结束时自然终止；调用方断连只移除自己的订阅。
    /// </summary>
    public async IAsyncEnumerable<string> SubscribeAsync(
        Guid runId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var state = _runs.GetOrAdd(runId, _ => new RunState());

        List<string> replay;
        var channel = Channel.CreateUnbounded<string>();
        lock (state)
        {
            replay = state.Buffer.ToList();
            if (!state.Completed)
            {
                state.Subscribers.Add(channel);
            }
        }

        try
        {
            foreach (var cached in replay)
            {
                ct.ThrowIfCancellationRequested();
                yield return cached;
            }

            if (replay.Count > 0)
            {
                bool finished;
                lock (state) { finished = state.Completed; }
                if (finished) yield break;
            }

            await foreach (var live in channel.Reader.ReadAllAsync(ct))
            {
                yield return live;
            }
        }
        finally
        {
            lock (state)
            {
                state.Subscribers.Remove(channel);
            }
            channel.Writer.TryComplete();
        }
    }

    /// <summary>
    /// 运行是否仍在跟踪（决定重连时能否拿到实时事件，还是只能读库里的轨迹）。
    /// </summary>
    public bool IsTracked(Guid runId) => _runs.ContainsKey(runId);

    /// <summary>
    /// 回收已结束且无人订阅的运行，避免缓冲常驻内存。
    /// </summary>
    public void Trim()
    {
        var cutoff = DateTime.Now - _retain;
        foreach (var (id, state) in _runs)
        {
            bool removable;
            lock (state)
            {
                removable = state.Completed && state.CompletedAt < cutoff && state.Subscribers.Count == 0;
            }
            if (removable)
            {
                _runs.TryRemove(id, out _);
            }
        }
    }
}
