using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace H.Mcp.YunXiao;

public class YunXiaoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly YunXiaoOptions _options;
    private readonly ILogger<YunXiaoApiClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
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
        // 云效 API 使用 x-yunxiao-token 头进行 PAT 认证
        _httpClient.DefaultRequestHeaders.Add("x-yunxiao-token", _options.PersonalAccessToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// 获取单个工作项详情
    /// API: GET /oapi/v1/projex/organizations/{organizationId}/workitems/{workitemId}
    /// </summary>
    public async Task<string> GetWorkItemInfoAsync(
        string spaceIdentifier,
        string spaceType,
        string workitemId)
    {
        try
        {
            var url = $"/oapi/v1/projex/organizations/{_options.OrganizationId}/workitems/{Uri.EscapeDataString(workitemId)}";

            _logger.LogInformation("获取工作项详情: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("获取工作项失败: {StatusCode}, {Content}", response.StatusCode, content);
                return $"获取工作项失败: HTTP {response.StatusCode}, {content}";
            }

            // 检查响应是否为 JSON 格式
            if (IsHtmlResponse(content))
            {
                _logger.LogError("API 返回了非 JSON 响应");
                return "获取工作项失败: API 返回了非 JSON 响应，可能是认证失败或 URL 不正确";
            }

            var json = JsonSerializer.Deserialize<JsonElement>(content);

            // oapi 接口直接返回工作项对象
            if (json.ValueKind == JsonValueKind.Object)
            {
                return FormatWorkItem(json);
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取工作项详情时发生异常: {Message}", ex.Message);
            return $"获取工作项异常: {ex.Message}";
        }
    }

    /// <summary>
    /// 搜索工作项列表
    /// API: POST /oapi/v1/projex/organizations/{organizationId}/workitems:search
    /// </summary>
    public async Task<string> SearchWorkItemsAsync(
        string spaceIdentifier,
        string? keyword = null,
        string? category = null)
    {
        var url = $"/oapi/v1/projex/organizations/{_options.OrganizationId}/workitems:search";

        // 构建请求体
        var payload = new Dictionary<string, object?>
        {
            ["spaceId"] = spaceIdentifier,
            ["spaceType"] = "Project",
            ["page"] = 1,
            ["perPage"] = 20,
            ["orderBy"] = "gmtCreate",
            ["sort"] = "desc"
        };

        if (!string.IsNullOrEmpty(category))
        {
            payload["category"] = category;
        }
        else
        {
            payload["category"] = "Req"; // 默认搜索需求
        }

        // 构建搜索条件
        if (!string.IsNullOrEmpty(keyword))
        {
            var conditions = JsonSerializer.Serialize(new
            {
                conditionGroups = new[]
                {
                    new[]
                    {
                        new
                        {
                            className = "string",
                            fieldIdentifier = "subject",
                            format = "input",
                            @operator = "CONTAINS",
                            value = new[] { keyword }
                        }
                    }
                }
            });
            payload["conditions"] = conditions;
        }

        _logger.LogInformation("搜索工作项: {Url}, Payload: {Payload}", url, JsonSerializer.Serialize(payload));

        var response = await _httpClient.PostAsJsonAsync(url, payload, JsonOptions);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("搜索工作项失败: {StatusCode}, {Content}", response.StatusCode, content);
            return $"搜索工作项失败: HTTP {response.StatusCode}, {content}";
        }

        // 检查响应是否为 JSON 格式
        if (IsHtmlResponse(content))
        {
            _logger.LogError("API 返回了非 JSON 响应");
            return "搜索工作项失败: API 返回了非 JSON 响应，可能是认证失败或 URL 不正确";
        }

        var json = JsonSerializer.Deserialize<JsonElement>(content);

        // 响应可能是数组或包含 workitems 的对象
        if (json.ValueKind == JsonValueKind.Array)
        {
            var total = response.Headers.TryGetValues("x-total", out var totalValues)
                ? totalValues.FirstOrDefault()
                : null;
            var sb = new StringBuilder();
            sb.AppendLine($"找到 {total ?? json.GetArrayLength().ToString()} 个工作项：\n");

            foreach (var item in json.EnumerateArray())
            {
                sb.AppendLine(FormatWorkItemSummary(item));
                sb.AppendLine("---");
            }

            return sb.ToString();
        }

        if (json.ValueKind == JsonValueKind.Object && json.TryGetProperty("workitems", out var workitems) && workitems.ValueKind == JsonValueKind.Array)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"找到 {workitems.GetArrayLength()} 个工作项：\n");

            foreach (var item in workitems.EnumerateArray())
            {
                sb.AppendLine(FormatWorkItemSummary(item));
                sb.AppendLine("---");
            }

            return sb.ToString();
        }

        return content;
    }

    /// <summary>
    /// 获取组织下的项目列表
    /// API: GET /oapi/v1/projex/organizations/{organizationId}/projects
    /// </summary>
    public async Task<string> ListProjectsAsync()
    {
        try
        {
            var url = $"/oapi/v1/projex/organizations/{_options.OrganizationId}/projects";
            _logger.LogInformation("获取项目列表: {Url}", url);

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("获取项目列表失败: {StatusCode}, {Content}", response.StatusCode, content);
                // 尝试备用接口
                return await ListProjectsFallbackAsync();
            }

            if (IsHtmlResponse(content))
            {
                return "获取项目列表失败: API 返回了非 JSON 响应";
            }

            return FormatProjectsResponse(content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取项目列表时发生异常: {Message}", ex.Message);
            return $"获取项目列表异常: {ex.Message}";
        }
    }

    /// <summary>
    /// 备用接口获取项目列表
    /// </summary>
    private async Task<string> ListProjectsFallbackAsync()
    {
        // 尝试 /api/v2/projex/organizations/{orgId}/projects 路径
        var url = $"/api/v2/projex/organizations/{_options.OrganizationId}/projects";
        _logger.LogInformation("尝试备用接口获取项目列表: {Url}", url);

        var response = await _httpClient.GetAsync(url);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return $"获取项目列表失败（含备用接口）: HTTP {response.StatusCode}, {content}";
        }

        return FormatProjectsResponse(content);
    }

    private static string FormatProjectsResponse(string content)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            var sb = new StringBuilder();

            // 处理不同的响应格式
            if (json.ValueKind == JsonValueKind.Array)
            {
                sb.AppendLine($"找到 {json.GetArrayLength()} 个项目：\n");
                foreach (var project in json.EnumerateArray())
                {
                    FormatProjectSummary(sb, project);
                }
            }
            else if (json.ValueKind == JsonValueKind.Object)
            {
                // 可能包含 projects 或 items 或 result 属性
                if (json.TryGetProperty("projects", out var projects) && projects.ValueKind == JsonValueKind.Array)
                {
                    sb.AppendLine($"找到 {projects.GetArrayLength()} 个项目：\n");
                    foreach (var project in projects.EnumerateArray())
                    {
                        FormatProjectSummary(sb, project);
                    }
                }
                else if (json.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
                {
                    sb.AppendLine($"找到 {items.GetArrayLength()} 个项目：\n");
                    foreach (var project in items.EnumerateArray())
                    {
                        FormatProjectSummary(sb, project);
                    }
                }
                else
                {
                    sb.AppendLine($"原始响应:\n{json.GetRawText()}");
                }
            }
            else
            {
                sb.AppendLine($"原始响应:\n{content}");
            }

            return sb.ToString();
        }
        catch
        {
            return $"原始响应:\n{content}";
        }
    }

    private static void FormatProjectSummary(StringBuilder sb, JsonElement project)
    {
        if (project.TryGetProperty("name", out var name))
            sb.Append($"[{name.GetString()}]");
        else if (project.TryGetProperty("projectName", out var projectName))
            sb.Append($"[{projectName.GetString()}]");

        if (project.TryGetProperty("id", out var id))
            sb.Append($" (ID: {id.GetString()})");
        else if (project.TryGetProperty("projectId", out var projectId))
            sb.Append($" (ID: {projectId.GetString()})");

        if (project.TryGetProperty("identifier", out var identifier))
            sb.Append($" 前缀: {identifier.GetString()}");

        if (project.TryGetProperty("description", out var desc) && desc.ValueKind == JsonValueKind.String)
            sb.Append($" - {desc.GetString()}");

        sb.AppendLine();
        sb.AppendLine("---");
    }

    private static bool IsHtmlResponse(string content)
    {
        return !string.IsNullOrEmpty(content) && content.TrimStart().StartsWith("<");
    }

    private static string FormatWorkItem(JsonElement workitem)
    {
        var sb = new StringBuilder();

        if (workitem.TryGetProperty("subject", out var subject))
            sb.AppendLine($"标题: {subject.GetString()}");

        if (workitem.TryGetProperty("identifier", out var identifier))
            sb.AppendLine($"标识: {identifier.GetString()}");

        if (workitem.TryGetProperty("serialNumber", out var serialNumber))
            sb.AppendLine($"编号: {serialNumber.GetString()}");

        if (workitem.TryGetProperty("categoryIdentifier", out var category))
            sb.AppendLine($"类型: {category.GetString()}");

        if (workitem.TryGetProperty("status", out var status))
            sb.AppendLine($"状态: {status.GetString()}");

        if (workitem.TryGetProperty("assignedTo", out var assignedTo))
            sb.AppendLine($"负责人: {assignedTo.GetString()}");

        if (workitem.TryGetProperty("spaceName", out var spaceName))
            sb.AppendLine($"项目: {spaceName.GetString()}");

        if (workitem.TryGetProperty("gmtCreate", out var gmtCreate) && gmtCreate.ValueKind == JsonValueKind.Number)
            sb.AppendLine($"创建时间: {DateTimeOffset.FromUnixTimeMilliseconds(gmtCreate.GetInt64()).LocalDateTime:yyyy-MM-dd HH:mm}");

        if (workitem.TryGetProperty("gmtModified", out var gmtModified) && gmtModified.ValueKind == JsonValueKind.Number)
            sb.AppendLine($"修改时间: {DateTimeOffset.FromUnixTimeMilliseconds(gmtModified.GetInt64()).LocalDateTime:yyyy-MM-dd HH:mm}");

        if (workitem.TryGetProperty("document", out var document))
        {
            sb.AppendLine($"\n描述:\n{document.GetString()}");
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

        if (item.TryGetProperty("serialNumber", out var serialNumber))
            sb.Append($" #{serialNumber.GetString()}");

        if (item.TryGetProperty("status", out var status))
            sb.Append($" - 状态: {status.GetString()}");

        if (item.TryGetProperty("assignedTo", out var assignedTo))
            sb.Append($" - 负责人: {assignedTo.GetString()}");

        return sb.ToString();
    }
}
