using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 浏览器工具。两类能力并存：
/// 轻量档（静态 HttpClient：抓 HTML、抽文本、抽链接、探活）适合只需要读内容的场景；
/// 真浏览器档（Playwright 会话：导航、点击、输入、截图）处理需要 JS 渲染或交互的页面。
/// 会话状态不在本类字段里，而在单例 BrowserSessionPool——本类每次注册都会被重建。
/// </summary>
public class BrowserTool
{
    private const int SelectorMaxLength = 400;
    private const int ReadBudgetChars = 3300;

    private readonly BrowserSessionPool _pool;
    private readonly WorkbenchToolOptions _options;
    private readonly GitWorkspaceResolver _resolver;
    private readonly GitWorkspaceLocks _locks;
    private readonly ILogger<BrowserTool> _logger;

    public BrowserTool(
        IOptions<WorkbenchToolOptions> options,
        BrowserSessionPool pool,
        GitWorkspaceLocks locks,
        ILogger<BrowserTool> logger)
    {
        _options = options.Value;
        _pool = pool;
        _locks = locks;
        _resolver = new GitWorkspaceResolver(options);
        _logger = logger;
    }

    [Description("打开真浏览器会话并导航到网页（Playwright，会弹出浏览器窗口）。返回 sessionId，后续 BrowserNavigateAsync/BrowserClickAsync/BrowserTypeAsync/BrowserReadAsync/BrowserScreenshotAsync/BrowserCloseAsync 都要带上它。参数：url, timeoutSeconds。")]
    public async Task<string> BrowserOpenAsync(
        [Description("要打开的网页地址")] string url,
        [Description("导航超时秒数，默认 30")] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        if (!IsSafeUrl(url, out var urlError)) return ToolEnvelope.Fail(urlError!);

        try
        {
            var session = await _pool.OpenAsync(url.Trim(), ClampTimeout(timeoutSeconds), cancellationToken);
            return ToolEnvelope.Ok(new
            {
                sessionId = session.Id,
                url = session.Page.Url,
                title = await session.Page.TitleAsync(),
                activeSessions = _pool.ActiveCount,
                note = "用完后调用 BrowserCloseAsync 释放"
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "浏览器会话打开失败: {Url}", url);
            return ToolEnvelope.Fail($"打开浏览器失败: {FirstLine(ex.Message)}");
        }
    }

    [Description("在已有浏览器会话里导航到新地址。参数：sessionId, url, timeoutSeconds。")]
    public async Task<string> BrowserNavigateAsync(
        [Description("BrowserOpenAsync 返回的会话标识")] string sessionId,
        [Description("目标地址")] string url,
        [Description("导航超时秒数，默认 30")] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        if (_pool.Find(sessionId) is not { } session) return SessionMissing(sessionId);
        if (!IsSafeUrl(url, out var urlError)) return ToolEnvelope.Fail(urlError!);

        try
        {
            await session.Page.GotoAsync(url.Trim(), new PageGotoOptions
            {
                Timeout = ClampTimeout(timeoutSeconds) * 1000,
                WaitUntil = WaitUntilState.DOMContentLoaded
            });

            return ToolEnvelope.Ok(new
            {
                sessionId = session.Id,
                url = session.Page.Url,
                title = await session.Page.TitleAsync()
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"导航失败: {FirstLine(ex.Message)}");
        }
    }

    [Description("点击页面上的元素（CSS 选择器，也支持 Playwright 的 text= / role= 语法）。参数：sessionId, selector, timeoutSeconds。")]
    public async Task<string> BrowserClickAsync(
        [Description("会话标识")] string sessionId,
        [Description("元素选择器，例如 #submit 或 text=登录")] string selector,
        [Description("等待超时秒数，默认 10")] int timeoutSeconds = 10,
        CancellationToken cancellationToken = default)
    {
        if (_pool.Find(sessionId) is not { } session) return SessionMissing(sessionId);
        if (!IsSafeSelector(selector, out var selectorError)) return ToolEnvelope.Fail(selectorError!);

        try
        {
            await session.Page.Locator(selector.Trim())
                .ClickAsync(new LocatorClickOptions { Timeout = ClampTimeout(timeoutSeconds) * 1000 });

            return ToolEnvelope.Ok(new
            {
                sessionId = session.Id,
                clicked = selector.Trim(),
                url = session.Page.Url,
                title = await session.Page.TitleAsync()
            });
        }
        catch (TimeoutException)
        {
            return ToolEnvelope.Fail($"未找到可点击元素或点击超时: {selector}（可先用 BrowserReadAsync 看页面结构）");
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"点击失败: {FirstLine(ex.Message)}");
        }
    }

    [Description("在页面输入框里填写文本。参数：sessionId, selector, text, clearFirst（默认先清空）。")]
    public async Task<string> BrowserTypeAsync(
        [Description("会话标识")] string sessionId,
        [Description("输入框选择器")] string selector,
        [Description("要输入的文本")] string text,
        [Description("是否先清空原有内容")] bool clearFirst = true,
        CancellationToken cancellationToken = default)
    {
        if (_pool.Find(sessionId) is not { } session) return SessionMissing(sessionId);
        if (!IsSafeSelector(selector, out var selectorError)) return ToolEnvelope.Fail(selectorError!);

        var value = text ?? "";
        if (value.Length > 4000) return ToolEnvelope.Fail("输入文本超过 4000 字");

        try
        {
            var locator = session.Page.Locator(selector.Trim());
            if (clearFirst) await locator.FillAsync("");
            await locator.FillAsync(value);

            return ToolEnvelope.Ok(new
            {
                sessionId = session.Id,
                target = selector.Trim(),
                chars = value.Length
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"输入失败: {FirstLine(ex.Message)}");
        }
    }

    [Description("读取浏览器页面渲染后的可见文本（含 JS 渲染结果，比 ExtractTextAsync 的静态抓取更真实）。参数：sessionId, selector（留空读整页 body）, maxChars（默认 3000）。")]
    public async Task<string> BrowserReadAsync(
        [Description("会话标识")] string sessionId,
        [Description("只读某个元素的 selector，留空读整页")] string? selector = null,
        [Description("最多返回字符数")] int maxChars = 3000,
        CancellationToken cancellationToken = default)
    {
        if (_pool.Find(sessionId) is not { } session) return SessionMissing(sessionId);

        var limit = Math.Clamp(maxChars > 0 ? maxChars : 3000, 200, ReadBudgetChars);

        try
        {
            string raw;
            if (string.IsNullOrWhiteSpace(selector))
            {
                raw = await session.Page.Locator("body").InnerTextAsync();
            }
            else
            {
                if (!IsSafeSelector(selector, out var selectorError)) return ToolEnvelope.Fail(selectorError!);
                raw = await session.Page.Locator(selector.Trim()).InnerTextAsync();
            }

            var text = NormalizeWhitespace(raw);
            var truncated = text.Length > limit;

            return ToolEnvelope.Ok(new
            {
                sessionId = session.Id,
                url = session.Page.Url,
                title = await session.Page.TitleAsync(),
                text = truncated ? text[..limit] : text,
                truncated
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"读取页面失败: {FirstLine(ex.Message)}");
        }
    }

    [Description("对浏览器页面截图并存入服务端工作目录（PNG）。参数：sessionId, fileName（相对路径，留空自动命名），repo（留空则存到工作目录 outputs 下）, fullPage（默认整页）。")]
    public async Task<string> BrowserScreenshotAsync(
        [Description("会话标识")] string sessionId,
        [Description("图片相对路径，必须以 .png 结尾，留空则自动命名")] string? fileName = null,
        [Description("已克隆的仓库目录名或地址，可空")] string? repo = null,
        [Description("是否整页截图")] bool fullPage = true,
        CancellationToken cancellationToken = default)
    {
        if (_pool.Find(sessionId) is not { } session) return SessionMissing(sessionId);

        var name = string.IsNullOrWhiteSpace(fileName)
            ? $"screenshots/page-{DateTime.Now:yyyyMMdd-HHmmss}-{session.Id}.png"
            : fileName.Trim();

        if (!name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
        {
            return ToolEnvelope.Fail("fileName 必须以 .png 结尾");
        }

        if (!_resolver.TryResolveOutputPath(repo, name, out var fullPath, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath!)!);
            await session.Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = fullPath,
                FullPage = fullPage
            });

            return ToolEnvelope.Ok(new
            {
                file = Path.GetRelativePath(_resolver.WorkDir, fullPath!).Replace('\\', '/'),
                url = session.Page.Url,
                sizeBytes = new FileInfo(fullPath!).Length,
                fullPage
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"截图失败: {FirstLine(ex.Message)}");
        }
    }

    [Description("关闭浏览器会话并释放页面。参数：sessionId。")]
    public async Task<string> BrowserCloseAsync(
        [Description("会话标识")] string sessionId,
        CancellationToken cancellationToken = default)
    {
        var closed = await _pool.CloseAsync(sessionId);
        return closed
            ? ToolEnvelope.Ok(new { closed = sessionId, activeSessions = _pool.ActiveCount })
            : ToolEnvelope.Fail($"会话 {sessionId} 不存在或已关闭");
    }

    private static string SessionMissing(string? sessionId) =>
        ToolEnvelope.Fail($"浏览器会话 {sessionId} 不存在或已超时回收，请重新调用 BrowserOpenAsync");

    private static bool IsSafeUrl(string? url, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(url))
        {
            error = "url 不能为空";
            return false;
        }

        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var parsed) ||
            (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps))
        {
            error = $"url 必须是 http/https 绝对地址: {url}";
            return false;
        }

        return true;
    }

    private static bool IsSafeSelector(string? selector, out string? error)
    {
        error = null;
        var value = (selector ?? "").Trim();
        if (value.Length == 0)
        {
            error = "selector 不能为空";
            return false;
        }

        if (value.Length > SelectorMaxLength)
        {
            error = $"selector 超过 {SelectorMaxLength} 字符，请换更短的定位方式";
            return false;
        }

        return true;
    }

    private int ClampTimeout(int timeoutSeconds) =>
        Math.Clamp(timeoutSeconds > 0 ? timeoutSeconds : Math.Max(5, _options.Browser.NavigationTimeoutSeconds), 1, 120);

    private static string FirstLine(string message) =>
        string.IsNullOrEmpty(message) ? "未知错误" : message.Split('\n')[0].Trim();

    private static string NormalizeWhitespace(string text) =>
        System.Text.RegularExpressions.Regex.Replace(text ?? "", @"\s{2,}", " ").Trim();

    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
        AllowAutoRedirect = true,
        UseCookies = true,
        CookieContainer = new CookieContainer()
    })
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    static BrowserTool()
    {
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8");
    }

    [Description("访问网页并获取内容。参数：url, method, headers, timeoutSeconds, cancellationToken。")]
    public static async Task<string> FetchPageAsync(
        [Description("目标网页 URL")] string url,
        [Description("HTTP 方法，支持 GET/POST，默认 GET")] string method = "GET",
        [Description("自定义请求头字典（JSON 格式），可为 null")] string? headers = null,
        [Description("POST 请求体，可为 null")] string? body = null,
        [Description("请求超时（秒），默认 30 秒")] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 BrowserTool.FetchPageAsync -> {url}");

        try
        {
            using var request = new HttpRequestMessage(new HttpMethod(method.ToUpper()), url);

            // 添加自定义 headers
            if (!string.IsNullOrWhiteSpace(headers))
            {
                var headerDict = JsonSerializer.Deserialize<Dictionary<string, string>>(headers);
                if (headerDict != null)
                {
                    foreach (var kv in headerDict)
                    {
                        request.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }
            }

            // 设置 POST body
            if (method.ToUpper() == "POST" && !string.IsNullOrWhiteSpace(body))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

            using var response = await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);

            var payload = new
            {
                statusCode = (int)response.StatusCode,
                statusDescription = response.StatusCode.ToString(),
                content,
                url = response.RequestMessage?.RequestUri?.ToString()
            };

            return response.IsSuccessStatusCode
                ? ToolEnvelope.Ok(payload)
                : ToolEnvelope.Fail($"访问 {url} 返回 HTTP {(int)response.StatusCode} ({response.StatusCode})");
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"访问网页失败: {ex.Message}");
        }
    }

    [Description("提取网页的纯文本内容（已移除脚本与标签，最长 10000 字）。参数：url, timeoutSeconds, cancellationToken。")]
    public static async Task<string> ExtractTextAsync(
        [Description("目标网页 URL")] string url,
        [Description("请求超时（秒），默认 30 秒")] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 BrowserTool.ExtractTextAsync -> {url}");

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

            using var response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
            var html = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return ToolEnvelope.Fail($"提取文本失败：{url} 返回 HTTP {(int)response.StatusCode}");
            }

            // 简单 HTML 转文本（移除标签）
            var text = HtmlToText(html);

            return ToolEnvelope.Ok(new
            {
                url,
                text = text.Length > 10000 ? text[..10000] + "..." : text,
                truncated = text.Length > 10000
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"提取文本失败: {ex.Message}");
        }
    }

    [Description("获取网页的链接列表。参数：url, timeoutSeconds, cancellationToken。")]
    public static async Task<string> ExtractLinksAsync(
        [Description("目标网页 URL")] string url,
        [Description("请求超时（秒），默认 30 秒")] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 BrowserTool.ExtractLinksAsync -> {url}");

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

            using var response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
            var html = await response.Content.ReadAsStringAsync(cts.Token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return ToolEnvelope.Fail($"提取链接失败：{url} 返回 HTTP {(int)response.StatusCode}");
            }

            // 简单提取链接
            var links = ExtractLinksFromHtml(html, url);

            return ToolEnvelope.Ok(new
            {
                url,
                linkCount = links.Count,
                links = links.Take(100).ToList() // 限制返回数量
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"提取链接失败: {ex.Message}");
        }
    }

    [Description("检查网页是否可访问。参数：url, timeoutSeconds, cancellationToken。")]
    public static async Task<string> CheckUrlAsync(
        [Description("目标网页 URL")] string url,
        [Description("请求超时（秒），默认 10 秒")] int timeoutSeconds = 10,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 BrowserTool.CheckUrlAsync -> {url}");

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

            var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request, cts.Token).ConfigureAwait(false);

            // "不可达"是本工具要回报的结论，不是工具自身失败，故不占用 success=false 语义
            return ToolEnvelope.Ok(new
            {
                url,
                reachable = response.IsSuccessStatusCode,
                statusCode = (int)response.StatusCode,
                statusDescription = response.StatusCode.ToString()
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Ok(new
            {
                url,
                reachable = false,
                statusCode = 0,
                statusDescription = ex.Message
            });
        }
    }

    /// <summary>
    /// 简单 HTML 转文本
    /// </summary>
    private static string HtmlToText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // 移除 script 和 style 标签及其内容
        html = System.Text.RegularExpressions.Regex.Replace(html, @"<script[^>]*>[\s\S]*?</script>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        html = System.Text.RegularExpressions.Regex.Replace(html, @"<style[^>]*>[\s\S]*?</style>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // 移除所有 HTML 标签
        var text = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", " ");

        // 处理 HTML 实体
        text = System.Net.WebUtility.HtmlDecode(text);

        // 合并多个空格和空行
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

        return text;
    }

    /// <summary>
    /// 从 HTML 中提取链接
    /// </summary>
    private static List<string> ExtractLinksFromHtml(string html, string baseUrl)
    {
        var links = new List<string>();
        var uri = new Uri(baseUrl);

        // 匹配 href 属性
        var matches = System.Text.RegularExpressions.Regex.Matches(html, @"href=[""']([^""']+)[""']", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var href = match.Groups[1].Value;

            try
            {
                // 转换为绝对 URL
                var absoluteUri = new Uri(uri, href);
                links.Add(absoluteUri.ToString());
            }
            catch
            {
                // 忽略无效 URL
            }
        }

        return links.Distinct().ToList();
    }
}
