namespace H.Util.Blazor;

/// <summary>
/// Toast 消息类型
/// </summary>
public enum HToastType
{
    Success,
    Error,
    Warning,
    Info
}

/// <summary>
/// 单条 Toast 消息
/// </summary>
public class HToastMessage
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public HToastType Type { get; init; }
    public string Content { get; init; } = string.Empty;
    public int DurationMs { get; init; } = 3000;
}

/// <summary>
/// 全局 Toast 提示服务（Scoped 生命周期，自动消失）
/// </summary>
public class HToastService
{
    private readonly List<HToastMessage> _messages = [];
    private readonly Dictionary<string, CancellationTokenSource> _timers = [];

    /// <summary>
    /// 消息列表变更时触发
    /// </summary>
    public event Action? OnChange;

    public IReadOnlyList<HToastMessage> Messages => _messages;

    public void Success(string message, int durationMs = 3000) => Show(HToastType.Success, message, durationMs);
    public void Error(string message, int durationMs = 3000) => Show(HToastType.Error, message, durationMs);
    public void Warning(string message, int durationMs = 3000) => Show(HToastType.Warning, message, durationMs);
    public void Info(string message, int durationMs = 3000) => Show(HToastType.Info, message, durationMs);

    private void Show(HToastType type, string message, int durationMs = 3000)
    {
        var msg = new HToastMessage { Type = type, Content = message, DurationMs = durationMs };
        _messages.Add(msg);
        OnChange?.Invoke();

        if (durationMs > 0)
        {
            var cts = new CancellationTokenSource();
            _timers[msg.Id] = cts;
            _ = AutoDismissAsync(msg.Id, durationMs, cts.Token);
        }
    }

    private void Remove(string id)
    {
        _messages.RemoveAll(m => m.Id == id);
        if (_timers.Remove(id, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
        OnChange?.Invoke();
    }

    private async Task AutoDismissAsync(string id, int delayMs, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delayMs, ct);
            Remove(id);
        }
        catch (TaskCanceledException)
        {
            // 已取消，忽略
        }
    }
}
