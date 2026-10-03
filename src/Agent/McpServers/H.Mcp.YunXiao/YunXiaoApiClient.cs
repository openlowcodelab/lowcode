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
        // 云效 API 使用 x-yunxiao-token 头进行 PAT 认证。
        // 令牌为空时不写这个头：与其发一个注定 401 的请求让上层看到"流水线不存在"，
        // 不如在 ConfigurationError 里直接说明缺什么配置。
        if (!string.IsNullOrWhiteSpace(_options.PersonalAccessToken))
        {
            _httpClient.DefaultRequestHeaders.Add("x-yunxiao-token", _options.PersonalAccessToken);
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    /// 中心版接口要求企业 ID 与个人令牌都在配置里；缺任一项时工具层直接返回这条可执行提示
    /// </summary>
    public string? ConfigurationError
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_options.PersonalAccessToken))
            {
                return "未配置 YunXiao:PersonalAccessToken（云效个人访问令牌）";
            }

            if (string.IsNullOrWhiteSpace(_options.OrganizationId))
            {
                return "未配置 YunXiao:OrganizationId（云效企业 ID，中心版流水线接口必需）";
            }

            return null;
        }
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

    // ==================== 流水线（Flow，只读） ====================
    // 契约来自云效开发者文档：中心版路径带 organizations/{organizationId}，
    // 鉴权头 x-yunxiao-token，分页信息在响应头 x-total / x-page / x-per-page / x-total-pages。
    // 本类刻意只做读操作——触发与重跑会动生产流水线，等审批门能覆盖 MCP 工具后再接。

    private const int FlowOutputBudget = 3200;

    private async Task<(bool Ok, JsonElement Json, string Pagination, string Error)> GetFlowAsync(
        string path, string label, CancellationToken ct = default)
    {
        if (ConfigurationError is { } configError)
        {
            return (false, default, "", $"{label}不可用：{configError}。补齐配置并重启宿主后重试");
        }

        try
        {
            var response = await _httpClient.GetAsync(path, ct);
            var content = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("{Label} 失败: {Status}, {Content}", label, response.StatusCode, content);
                return (false, default, "", $"{label}失败: HTTP {(int)response.StatusCode}, {Trim(content, 400)}");
            }

            if (IsHtmlResponse(content))
            {
                return (false, default, "", $"{label}失败: 返回了非 JSON 响应，通常是令牌无效或企业 ID 不属于当前账号");
            }

            using var parsed = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "null" : content);
            return (true, parsed.RootElement.Clone(), ReadPagination(response), "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Label} 请求异常", label);
            return (false, default, "", $"{label}异常: {ex.Message}");
        }
    }

    /// <summary>
    /// GET /oapi/v1/flow/organizations/{orgId}/pipelines
    /// </summary>
    public async Task<string> ListPipelinesAsync(
        string? pipelineName, string? statusList, int page, int perPage, CancellationToken ct = default)
    {
        var query = new List<string>
        {
            $"page={Math.Clamp(page, 1, 10000)}",
            $"perPage={Math.Clamp(perPage, 1, 30)}"
        };

        if (!string.IsNullOrWhiteSpace(pipelineName))
        {
            query.Add($"pipelineName={Uri.EscapeDataString(pipelineName.Trim())}");
        }

        var statuses = CleanStatusList(statusList);
        if (statuses.Length > 0)
        {
            query.Add($"statusList={Uri.EscapeDataString(string.Join(",", statuses))}");
        }

        var (ok, json, pagination, error) = await GetFlowAsync(FlowPath("pipelines") + "?" + string.Join("&", query),
            "获取流水线列表", ct);
        if (!ok) return error;

        if (json.ValueKind != JsonValueKind.Array)
        {
            return "流水线列表响应格式异常：" + Trim(json.GetRawText(), 300);
        }

        var items = json.EnumerateArray().ToList();
        if (items.Count == 0)
        {
            var statusNote = statuses.Length > 0 ? string.Join(",", statuses) : "全部";
            var nameNote = string.IsNullOrWhiteSpace(pipelineName) ? "无" : pipelineName.Trim();
            return $"未找到流水线（名称筛选={nameNote}，状态={statusNote}）。{pagination}";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"共 {items.Count} 条流水线{pagination}：").AppendLine();

        foreach (var item in items)
        {
            var id = GetLong(item, "pipelineId");
            var name = GetString(item, "pipelineName");
            var created = GetLong(item, "createTime");
            sb.AppendLine($"- #{id} {name}" +
                          (created > 0 ? $"（创建于 {FormatTime(created)}）" : ""));
        }

        return Trim(sb.ToString(), FlowOutputBudget);
    }

    /// <summary>
    /// GET /oapi/v1/flow/organizations/{orgId}/pipelines/{pipelineId}/runs
    /// </summary>
    public async Task<string> ListPipelineRunsAsync(
        string pipelineId, string? status, int? triggerMode, int page, int perPage, CancellationToken ct = default)
    {
        if (!IsNumericId(pipelineId, "pipelineId", out var error)) return error!;

        var query = new List<string>
        {
            $"page={Math.Clamp(page, 1, 10000)}",
            $"perPage={Math.Clamp(perPage, 1, 30)}"
        };

        var cleanedStatus = (status ?? "").Trim().ToUpperInvariant();
        if (cleanedStatus is "FAIL" or "SUCCESS" or "RUNNING")
        {
            query.Add($"status={cleanedStatus}");
        }

        if (triggerMode is > 0)
        {
            query.Add($"triggerMode={triggerMode.Value}");
        }

        var (ok, json, pagination, runError) = await GetFlowAsync(
            FlowPath($"pipelines/{pipelineId!.Trim()}/runs") + "?" + string.Join("&", query),
            "获取流水线运行历史", ct);
        if (!ok) return runError;

        if (json.ValueKind != JsonValueKind.Array)
        {
            return "运行历史响应格式异常：" + Trim(json.GetRawText(), 300);
        }

        var items = json.EnumerateArray().ToList();
        if (items.Count == 0)
        {
            return $"流水线 {pipelineId} 没有符合条件的运行记录。{pagination}";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"流水线 {pipelineId} 的最近 {items.Count} 次运行{pagination}：").AppendLine();

        foreach (var item in items)
        {
            var runId = GetLong(item, "pipelineRunId");
            var state = GetString(item, "status");
            sb.AppendLine(
                $"- 运行 #{runId} " +
                $"{(state.Length > 0 ? state + " " : "")}" +
                $"触发={TriggerModeText(GetLong(item, "triggerMode"))} " +
                $"开始={FormatTime(GetLong(item, "startTime"))} " +
                $"结束={FormatTime(GetLong(item, "endTime"))}");
        }

        sb.AppendLine().Append("看某次运行的阶段与任务详情：GetPipelineRun(pipelineId, pipelineRunId)");
        return Trim(sb.ToString(), FlowOutputBudget);
    }

    /// <summary>
    /// GET /oapi/v1/flow/organizations/{orgId}/pipelines/{pipelineId}/runs/{pipelineRunId}
    /// </summary>
    public async Task<string> GetPipelineRunAsync(string pipelineId, string pipelineRunId, CancellationToken ct = default)
    {
        if (!IsNumericId(pipelineId, "pipelineId", out var error)) return error!;
        if (!IsNumericId(pipelineRunId, "pipelineRunId", out error)) return error!;

        return await ReadRunAsync(
            FlowPath($"pipelines/{pipelineId!.Trim()}/runs/{pipelineRunId!.Trim()}"),
            $"流水线 {pipelineId} 的运行 #{pipelineRunId}", ct);
    }

    /// <summary>
    /// GET /oapi/v1/flow/organizations/{orgId}/pipelines/{pipelineId}/runs/latestPipelineRun
    /// </summary>
    public async Task<string> GetLatestPipelineRunAsync(string pipelineId, CancellationToken ct = default)
    {
        if (!IsNumericId(pipelineId, "pipelineId", out var error)) return error!;

        return await ReadRunAsync(
            FlowPath($"pipelines/{pipelineId!.Trim()}/runs/latestPipelineRun"),
            $"流水线 {pipelineId} 最近一次运行", ct);
    }

    private async Task<string> ReadRunAsync(string path, string title, CancellationToken ct)
    {
        var (ok, json, _, error) = await GetFlowAsync(path, title, ct);
        if (!ok) return error;

        if (json.ValueKind != JsonValueKind.Object)
        {
            return title + "响应格式异常：" + Trim(json.GetRawText(), 300);
        }

        var sb = new StringBuilder();
        sb.Append(title)
          .Append($"：状态 {GetString(json, "status")}")
          .Append($"，触发方式 {TriggerModeText(GetLong(json, "triggerMode"))}")
          .Append($"，开始 {FormatTime(GetLong(json, "startTime"))}")
          .Append($"，结束 {FormatTime(GetLong(json, "endTime"))}")
          .AppendLine();

        if (json.TryGetProperty("sources", out var sources) && sources.ValueKind == JsonValueKind.Array)
        {
            foreach (var source in sources.EnumerateArray())
            {
                var data = source.TryGetProperty("data", out var d) && d.ValueKind == JsonValueKind.Object ? d : default;
                sb.AppendLine($"代码源：{GetString(source, "type")} {GetString(source, "name")} " +
                              $"分支={GetString(data, "branch")} 仓库={GetString(data, "repo")}");
            }
        }

        if (json.TryGetProperty("stages", out var stages) && stages.ValueKind == JsonValueKind.Array)
        {
            sb.AppendLine().AppendLine("阶段与任务：");
            foreach (var stage in stages.EnumerateArray())
            {
                sb.AppendLine($"- {GetString(stage, "name")}：{GetString(stage, "status")} " +
                              $"{FormatTime(GetLong(stage, "startTime"))} ~ {FormatTime(GetLong(stage, "endTime"))}");

                if (stage.TryGetProperty("stageInfo", out var info) &&
                    info.TryGetProperty("jobs", out var jobs) && jobs.ValueKind == JsonValueKind.Array)
                {
                    foreach (var job in jobs.EnumerateArray())
                    {
                        sb.AppendLine($"    · {GetString(job, "name")}：{GetString(job, "status")}" +
                                      (GetString(job, "runnerStatus").Length > 0
                                          ? $"（runner {GetString(job, "runnerStatus")}）"
                                          : ""));
                    }
                }
            }
        }

        sb.AppendLine().Append("注意：本工具只读，不会触发或重跑流水线。");
        return Trim(sb.ToString(), FlowOutputBudget);
    }

    private string FlowPath(string suffix) =>
        $"/oapi/v1/flow/organizations/{Uri.EscapeDataString(_options.OrganizationId.Trim())}/{suffix}";

    private static string ReadPagination(HttpResponseMessage response)
    {
        var parts = new List<string>();
        foreach (var header in new[] { "x-total", "x-page", "x-per-page", "x-total-pages" })
        {
            if (response.Headers.TryGetValues(header, out var values))
            {
                parts.Add($"{header[2..]}={string.Join("", values)}");
            }
        }

        return parts.Count == 0 ? "" : "（分页 " + string.Join(" ", parts) + "）";
    }

    private static bool IsNumericId(string? value, string name, out string error)
    {
        var trimmed = (value ?? "").Trim();
        var valid = trimmed.Length > 0 && trimmed.All(char.IsDigit);
        error = valid ? "" : $"{name} 必须是数字 ID（可先用 ListPipelines 查），收到：{value}";
        return valid;
    }

    private static string[] CleanStatusList(string? statusList)
    {
        const string allowed = "SUCCESS,RUNNING,FAIL,CANCELED,WAITING";
        return (statusList ?? "").Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().ToUpperInvariant())
            .Where(allowed.Contains)
            .Distinct()
            .ToArray();
    }

    private static string TriggerModeText(long mode) => mode switch
    {
        1 => "手动",
        2 => "定时",
        3 => "代码提交",
        5 => "流水线触发",
        6 => "Webhook",
        0 => "未知",
        _ => $"未知({mode})"
    };

    private static string FormatTime(long epochMillis)
    {
        if (epochMillis <= 0) return "-";
        try
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(epochMillis).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch (ArgumentOutOfRangeException)
        {
            return epochMillis.ToString();
        }
    }

    private static string GetString(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object &&
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static long GetLong(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(name, out var value)) return 0;

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetInt64(out var number) ? number : 0,
            JsonValueKind.String when long.TryParse(value.GetString(), out var parsed) => parsed,
            _ => 0
        };
    }

    private static string Trim(string text, int max)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= max ? text : text[..max] + "\n...[已截断]";
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
