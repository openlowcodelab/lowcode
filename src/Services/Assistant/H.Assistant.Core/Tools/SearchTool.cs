using System.ComponentModel;
using System.Net;
using System.Text.Json;

namespace H.Assistant.Core.Tools;

/// <summary>
/// 搜索工具 - 提供网络搜索功能
/// </summary>
public class SearchTool
{
    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
        AllowAutoRedirect = true
    })
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    static SearchTool()
    {
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/html, */*");
    }

    [Description("执行网络搜索。参数：query, searchEngine, resultCount, timeoutSeconds, cancellationToken。")]
    public static async Task<string> SearchAsync(
        [Description("搜索关键词")] string query,
        [Description("搜索引擎，支持 google/bing/baidu，默认 bing")] string searchEngine = "bing",
        [Description("返回结果数量，默认 10，最大 20")] int resultCount = 10,
        [Description("请求超时（秒），默认 30 秒")] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 SearchTool.SearchAsync -> {query} (engine: {searchEngine})");

        try
        {
            resultCount = Math.Min(Math.Max(1, resultCount), 20);

            var url = searchEngine.ToLower() switch
            {
                "google" => BuildGoogleSearchUrl(query, resultCount),
                "baidu" => BuildBaiduSearchUrl(query, resultCount),
                _ => BuildBingSearchUrl(query, resultCount)
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

            var html = await _httpClient.GetStringAsync(url, cts.Token).ConfigureAwait(false);

            var results = searchEngine.ToLower() switch
            {
                "google" => ParseGoogleResults(html),
                "baidu" => ParseBaiduResults(html),
                _ => ParseBingResults(html)
            };

            var result = new
            {
                Success = true,
                Query = query,
                Engine = searchEngine,
                ResultCount = results.Count,
                Results = results
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 搜索失败: {ex.Message}";
        }
    }

    [Description("搜索新闻。参数：query, language, resultCount, timeoutSeconds, cancellationToken。")]
    public static async Task<string> SearchNewsAsync(
        [Description("新闻关键词")] string query,
        [Description("语言，默认 zh-CN（中文）")] string language = "zh-CN",
        [Description("返回结果数量，默认 10")] int resultCount = 10,
        [Description("请求超时（秒），默认 30 秒")] int timeoutSeconds = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 SearchTool.SearchNewsAsync -> {query}");

        try
        {
            // 使用 Bing 新闻搜索
            var url = BuildBingNewsSearchUrl(query, language, resultCount);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

            var html = await _httpClient.GetStringAsync(url, cts.Token).ConfigureAwait(false);
            var results = ParseBingNewsResults(html);

            var result = new
            {
                Success = true,
                Query = query,
                Language = language,
                ResultCount = results.Count,
                Results = results
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 搜索新闻失败: {ex.Message}";
        }
    }

    private static string BuildBingSearchUrl(string query, int count)
    {
        return $"https://www.bing.com/search?q={Uri.EscapeDataString(query)}&count={count}";
    }

    private static string BuildGoogleSearchUrl(string query, int count)
    {
        return $"https://www.google.com/search?q={Uri.EscapeDataString(query)}&num={count}";
    }

    private static string BuildBaiduSearchUrl(string query, int count)
    {
        return $"https://www.baidu.com/s?wd={Uri.EscapeDataString(query)}&rn={count}";
    }

    private static string BuildBingNewsSearchUrl(string query, string language, int count)
    {
        return $"https://www.bing.com/news/search?q={Uri.EscapeDataString(query)}&qft=sortbydate&setlang={language}&count={count}";
    }

    /// <summary>
    /// 简单解析 Bing 搜索结果
    /// </summary>
    private static List<SearchResult> ParseBingResults(string html)
    {
        var results = new List<SearchResult>();

        try
        {
            // 提取搜索结果项（简化版）
            var pattern = @"<li[^>]*class=""b_algo""[^>]*>(.*?)</li>";
            var matches = System.Text.RegularExpressions.Regex.Matches(html, pattern, System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var itemHtml = match.Groups[1].Value;

                // 提取标题
                var titleMatch = System.Text.RegularExpressions.Regex.Match(itemHtml, @"<h2[^>]*>.*?<a[^>]*href=""([^""]+)""[^>]*>(.*?)</a>.*?</h2>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!titleMatch.Success) continue;

                var url = System.Net.WebUtility.HtmlDecode(titleMatch.Groups[1].Value);
                var title = StripHtml(titleMatch.Groups[2].Value);

                // 提取摘要
                var snippetMatch = System.Text.RegularExpressions.Regex.Match(itemHtml, @"<p[^>]*>(.*?)</p>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var snippet = snippetMatch.Success ? StripHtml(snippetMatch.Groups[1].Value) : "";

                results.Add(new SearchResult
                {
                    Title = title,
                    Url = url,
                    Snippet = snippet
                });
            }
        }
        catch
        {
            // 解析失败时返回空列表
        }

        // 如果正则解析失败，返回提示信息
        if (results.Count == 0)
        {
            results.Add(new SearchResult
            {
                Title = "搜索结果",
                Url = "",
                Snippet = "注意：由于 HTML 解析限制，建议配合专用搜索引擎 API 使用"
            });
        }

        return results;
    }

    /// <summary>
    /// 简单解析 Google 搜索结果
    /// </summary>
    private static List<SearchResult> ParseGoogleResults(string html)
    {
        var results = new List<SearchResult>();

        try
        {
            var pattern = @"<a[^>]*href=""(https?://[^""]+)""[^>]*>(.*?)</a>";
            var matches = System.Text.RegularExpressions.Regex.Matches(html, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var url = match.Groups[1].Value;

                // 过滤 Google 内部链接
                if (url.Contains("google.com") || url.Contains("gstatic.com") || url.Contains("youtube.com/redirect"))
                    continue;

                var title = StripHtml(match.Groups[2].Value);
                if (string.IsNullOrWhiteSpace(title) || title.Length < 10)
                    continue;

                results.Add(new SearchResult
                {
                    Title = title,
                    Url = url,
                    Snippet = ""
                });
            }
        }
        catch
        {
        }

        if (results.Count == 0)
        {
            results.Add(new SearchResult
            {
                Title = "搜索结果",
                Url = "",
                Snippet = "注意：由于 HTML 解析限制，建议配合专用搜索引擎 API 使用"
            });
        }

        return results;
    }

    /// <summary>
    /// 简单解析 Baidu 搜索结果
    /// </summary>
    private static List<SearchResult> ParseBaiduResults(string html)
    {
        var results = new List<SearchResult>();

        try
        {
            var pattern = @"<div[^>]*class=""result[^""]*""[^>]*>(.*?)</div>";
            var matches = System.Text.RegularExpressions.Regex.Matches(html, pattern, System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var itemHtml = match.Groups[1].Value;

                var titleMatch = System.Text.RegularExpressions.Regex.Match(itemHtml, @"<a[^>]*href=""([^""]+)""[^>]*>(.*?)</a>", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!titleMatch.Success) continue;

                var url = System.Net.WebUtility.HtmlDecode(titleMatch.Groups[1].Value);
                var title = StripHtml(titleMatch.Groups[2].Value);

                results.Add(new SearchResult
                {
                    Title = title,
                    Url = url,
                    Snippet = ""
                });
            }
        }
        catch
        {
        }

        if (results.Count == 0)
        {
            results.Add(new SearchResult
            {
                Title = "搜索结果",
                Url = "",
                Snippet = "注意：由于 HTML 解析限制，建议配合专用搜索引擎 API 使用"
            });
        }

        return results;
    }

    /// <summary>
    /// 解析 Bing 新闻结果
    /// </summary>
    private static List<SearchResult> ParseBingNewsResults(string html)
    {
        var results = new List<SearchResult>();

        try
        {
            var pattern = @"<a[^>]*href=""(https?://[^""]+)""[^>]*>(.*?)</a>";
            var matches = System.Text.RegularExpressions.Regex.Matches(html, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var url = match.Groups[1].Value;
                var title = StripHtml(match.Groups[2].Value);

                if (string.IsNullOrWhiteSpace(title) || title.Length < 10)
                    continue;

                if (url.Contains("bing.com"))
                    continue;

                results.Add(new SearchResult
                {
                    Title = title,
                    Url = url,
                    Snippet = ""
                });
            }
        }
        catch
        {
        }

        if (results.Count == 0)
        {
            results.Add(new SearchResult
            {
                Title = "新闻搜索结果",
                Url = "",
                Snippet = "注意：由于 HTML 解析限制，建议配合专用新闻 API 使用"
            });
        }

        return results;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var text = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", " ");
        text = System.Net.WebUtility.HtmlDecode(text);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
        return text;
    }

    private class SearchResult
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Snippet { get; set; } = "";
    }
}
