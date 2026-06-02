using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace H.YunXiaoMcpServer;

public class YunXiaoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly YunXiaoOptions _options;
    private readonly ILogger<YunXiaoApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public YunXiaoApiClient(
        IHttpClientFactory httpClientFactory,
        IOptions<YunXiaoOptions> options,
        ILogger<YunXiaoApiClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("YunXiao");
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.Endpoint);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.PersonalAccessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// 获取单个工作项详情
    /// API: GET /organization/{organizationId}/workitems
    /// </summary>
    public async Task<string> GetWorkItemInfoAsync(
        string spaceIdentifier,
        string spaceType,
        string workitemId)
    {
        var url = $"/organization/{_options.OrganizationId}/workitems" +
                  $"?spaceIdentifier={Uri.EscapeDataString(spaceIdentifier)}" +
                  $"&spaceType={Uri.EscapeDataString(spaceType)}" +
                  $"&id={Uri.EscapeDataString(workitemId)}";

        _logger.LogInformation("获取工作项详情: {Url}", url);

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("获取工作项失败: {StatusCode}, {Content}", response.StatusCode, content);
            return $"获取工作项失败: HTTP {response.StatusCode}, {content}";
        }

        var json = JsonSerializer.Deserialize<JsonElement>(content);

        if (json.TryGetProperty("workitem", out var workitem))
        {
            return FormatWorkItem(workitem);
        }

        return content;
    }

    /// <summary>
    /// 搜索工作项列表
    /// API: POST /organization/{organizationId}/workitems/list
    /// </summary>
    public async Task<string> SearchWorkItemsAsync(
        string spaceIdentifier,
        string? keyword = null,
        string? category = null)
    {
        var url = $"/organization/{_options.OrganizationId}/workitems/list" +
                  $"?spaceIdentifier={Uri.EscapeDataString(spaceIdentifier)}" +
                  $"&spaceType=Project";

        if (!string.IsNullOrEmpty(category))
        {
            url += $"&category={Uri.EscapeDataString(category)}";
        }

        var requestBody = new
        {
            conditions = string.IsNullOrEmpty(keyword)
                ? null
                : new[]
                {
                    new
                    {
                        fieldIdentifier = "subject",
                        @operator = "CONTAINS",
                        value = new[] { keyword }
                    }
                }
        };

        _logger.LogInformation("搜索工作项: {Url}", url);

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, JsonOptions);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("搜索工作项失败: {StatusCode}, {Content}", response.StatusCode, content);
            return $"搜索工作项失败: HTTP {response.StatusCode}, {content}";
        }

        var json = JsonSerializer.Deserialize<JsonElement>(content);

        if (json.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Array)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"找到 {result.GetArrayLength()} 个工作项：\n");

            foreach (var item in result.EnumerateArray())
            {
                sb.AppendLine(FormatWorkItemSummary(item));
                sb.AppendLine("---");
            }

            return sb.ToString();
        }

        return content;
    }

    private static string FormatWorkItem(JsonElement workitem)
    {
        var sb = new StringBuilder();

        if (workitem.TryGetProperty("subject", out var subject))
            sb.AppendLine($"标题: {subject.GetString()}");

        if (workitem.TryGetProperty("identifier", out var identifier))
            sb.AppendLine($"标识: {identifier.GetString()}");

        if (workitem.TryGetProperty("categoryIdentifier", out var category))
            sb.AppendLine($"类型: {category.GetString()}");

        if (workitem.TryGetProperty("statusIdentifier", out var status))
            sb.AppendLine($"状态: {status.GetString()}");

        if (workitem.TryGetProperty("assignedTo", out var assignedTo))
            sb.AppendLine($"负责人: {assignedTo.GetString()}");

        if (workitem.TryGetProperty("gmtCreate", out var gmtCreate))
            sb.AppendLine($"创建时间: {gmtCreate.GetString()}");

        if (workitem.TryGetProperty("gmtModified", out var gmtModified))
            sb.AppendLine($"修改时间: {gmtModified.GetString()}");

        if (workitem.TryGetProperty("description", out var description))
        {
            sb.AppendLine($"\n描述:\n{description.GetString()}");
        }

        // 返回完整的 JSON 以便 AI 获取更多结构化信息
        sb.AppendLine($"\n完整数据:\n{workitem.GetRawText()}");

        return sb.ToString();
    }

    private static string FormatWorkItemSummary(JsonElement item)
    {
        var sb = new StringBuilder();

        if (item.TryGetProperty("subject", out var subject))
            sb.Append($"[{subject.GetString()}]");

        if (item.TryGetProperty("identifier", out var id))
            sb.Append($" (ID: {id.GetString()})");

        if (item.TryGetProperty("statusIdentifier", out var status))
            sb.Append($" - 状态: {status.GetString()}");

        if (item.TryGetProperty("assignedTo", out var assignedTo))
            sb.Append($" - 负责人: {assignedTo.GetString()}");

        return sb.ToString();
    }
}
