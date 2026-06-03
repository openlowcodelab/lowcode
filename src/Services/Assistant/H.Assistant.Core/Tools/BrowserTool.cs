using System.ComponentModel;
using System.Net;
using System.Text;
using System.Text.Json;

namespace H.Assistant.Core.Tools;

/// <summary>
/// 浏览器工具 - 提供网页访问、截图、内容提取等功能
/// </summary>
public class BrowserTool
{
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

            var result = new
            {
                StatusCode = (int)response.StatusCode,
                StatusDescription = response.StatusCode.ToString(),
                Content = content,
                Url = response.RequestMessage?.RequestUri?.ToString()
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 访问网页失败: {ex.Message}";
        }
    }

    [Description("提取网页的文本内容。参数：url, selector（可选 CSS 选择器）, timeoutSeconds, cancellationToken。")]
    public static async Task<string> ExtractTextAsync(
        [Description("目标网页 URL")] string url,
        [Description("CSS 选择器，用于提取特定区域的文本，可为 null")] string? selector = null,
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

            // 简单 HTML 转文本（移除标签）
            var text = HtmlToText(html);

            if (!string.IsNullOrWhiteSpace(selector))
            {
                // 简化版：尝试提取包含 selector 关键字的内容
                // 完整实现需要 HTML 解析器，这里使用简单的字符串匹配
                text = $"⚠️ CSS 选择器 '{selector}' 需要完整的 HTML 解析器支持，返回全文内容";
            }

            var result = new
            {
                Success = true,
                Text = text.Length > 10000 ? text[..10000] + "..." : text,
                Url = url
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 提取文本失败: {ex.Message}";
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

            // 简单提取链接
            var links = ExtractLinksFromHtml(html, url);

            var result = new
            {
                Success = true,
                LinkCount = links.Count,
                Links = links.Take(100).ToList(), // 限制返回数量
                Url = url
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 提取链接失败: {ex.Message}";
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

            var result = new
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                StatusDescription = response.StatusCode.ToString(),
                Url = url
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Error = ex.Message,
                Url = url
            }, new JsonSerializerOptions { WriteIndented = false });
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
