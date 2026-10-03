using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace H.Workbench.Core.Tools.Internal;

/// <summary>
/// 浏览器会话池。两件事必须由它承担，工具类自己做不到：
/// 1) Playwright/浏览器进程要跨多次工具调用存活——ToolRegistry 每次注册都会用
///    ActivatorUtilities.CreateInstance 新建工具实例，存在工具字段里的页面下次就没了；
/// 2) 会话按 id 隔离——宿主允许 MaxConcurrentRuns 路并发执行，
///    如果所有员工共用"当前页面"，两个执行会互相把对方的页面踩掉。
/// </summary>
public sealed class BrowserSessionPool : IAsyncDisposable
{
    private readonly WorkbenchToolOptions _options;
    private readonly ILogger<BrowserSessionPool> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly ConcurrentDictionary<string, BrowserSession> _sessions =
        new(StringComparer.OrdinalIgnoreCase);

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private string? _launchedWith;

    public BrowserSessionPool(IOptions<WorkbenchToolOptions> options, ILogger<BrowserSessionPool> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public int ActiveCount => _sessions.Count;

    /// <summary>
    /// 新开一个会话并导航到 url。浏览器优先用系统已装的 Chrome/Edge，
    /// 都没有时回落到 Playwright 自带 Chromium（未下载过则会失败并给出可执行建议）。
    /// </summary>
    public async Task<BrowserSession> OpenAsync(string url, int timeoutSeconds, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await EnsureBrowserAsync(ct);
            SweepIdleSessions();

            var max = Math.Max(1, _options.Browser.MaxSessions);
            if (_sessions.Count >= max)
            {
                throw new InvalidOperationException(
                    $"并发浏览器会话已达上限 {max}，请先用 BrowserCloseAsync 关掉不用的会话");
            }

            var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            });

            var page = await context.NewPageAsync();
            await page.GotoAsync(url, new PageGotoOptions
            {
                Timeout = Math.Clamp(timeoutSeconds, 1, 120) * 1000,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            var session = new BrowserSession(Guid.NewGuid().ToString("N")[..12], context, page);
            _sessions[session.Id] = session;
            _logger.LogInformation("浏览器会话 {SessionId} 已打开 {Url}", session.Id, url);
            return session;
        }
        finally
        {
            _gate.Release();
        }
    }

    public BrowserSession? Find(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return null;
        if (!_sessions.TryGetValue(sessionId.Trim(), out var session)) return null;
        session.Touch();
        return session;
    }

    public async Task<bool> CloseAsync(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId)) return false;
        if (!_sessions.TryRemove(sessionId.Trim(), out var session)) return false;
        await session.DisposeAsync();
        return true;
    }

    private async Task EnsureBrowserAsync(CancellationToken ct)
    {
        if (_browser != null) return;

        _playwright ??= await Playwright.CreateAsync();

        var candidates = new List<string?>
        {
            string.IsNullOrWhiteSpace(_options.Browser.Channel) ? null : _options.Browser.Channel,
            "chrome",
            "msedge",
            null
        };

        string? lastError = null;
        foreach (var channel in candidates.Distinct())
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = _options.Browser.Headless,
                    Channel = channel
                });

                _browser = browser;
                _launchedWith = channel ?? "bundled-chromium";
                _logger.LogInformation("浏览器已启动：channel={Channel}, headless={Headless}",
                    _launchedWith, _options.Browser.Headless);
                return;
            }
            catch (Exception ex)
            {
                lastError = ex.Message.Split('\n')[0];
                _logger.LogDebug(ex, "浏览器 channel={Channel} 启动失败，尝试下一个", channel ?? "(bundled)");
            }
        }

        throw new InvalidOperationException(
            "无法启动浏览器（已尝试系统 Chrome、msedge 与内置 Chromium）。" +
            "可安装 Chrome/Edge，或在项目目录执行 dotnet playwright install chromium 下载内置浏览器。最后一次错误：" + lastError);
    }

    private void SweepIdleSessions()
    {
        var idle = TimeSpan.FromSeconds(Math.Max(30, _options.Browser.IdleTimeoutSeconds));
        foreach (var (id, session) in _sessions)
        {
            if (DateTime.UtcNow - session.LastUsed < idle) continue;
            if (_sessions.TryRemove(id, out var removed))
            {
                _ = Task.Run(async () => await removed.DisposeAsync());
                _logger.LogInformation("浏览器会话 {SessionId} 空闲超时已回收", id);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var (id, session) in _sessions)
        {
            _sessions.TryRemove(id, out _);
            try { await session.DisposeAsync(); } catch { /* 关闭尽力而为 */ }
        }

        if (_browser != null)
        {
            try { await _browser.CloseAsync(); } catch { /* 同上 */ }
            _browser = null;
        }

        _playwright?.Dispose();
        _gate.Dispose();
    }
}

/// <summary>
/// 一次浏览器会话 = 一个独立 Context + 一个 Page，员工拿到 sessionId 后按它操作
/// </summary>
public sealed class BrowserSession
{
    public BrowserSession(string id, IBrowserContext context, IPage page)
    {
        Id = id;
        Context = context;
        Page = page;
        LastUsed = DateTime.UtcNow;
    }

    public string Id { get; }
    public IBrowserContext Context { get; }
    public IPage Page { get; }
    public DateTime LastUsed { get; private set; }

    public void Touch() => LastUsed = DateTime.UtcNow;

    public async ValueTask DisposeAsync()
    {
        try { await Context.CloseAsync(); } catch { /* 页面可能已崩，关闭只能尽力而为 */ }
    }
}
