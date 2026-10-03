using ModelContextProtocol.Server;
using System.ComponentModel;

namespace H.Mcp.YunXiao;

[McpServerToolType]
public class YunXiaoMcpTools
{
    private readonly YunXiaoApiClient _apiClient;

    public YunXiaoMcpTools(YunXiaoApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [McpServerTool, Description("获取云效工作项详情。输入项目空间标识符、空间类型和工作项ID，返回工作项的标题、描述、状态、负责人等完整信息。")]
    public async Task<string> GetWorkItemInfo(
        [Description("项目空间标识符（spaceIdentifier），通常是项目ID或项目路径")] string spaceIdentifier,
        [Description("工作项ID（workitemId），工作项的唯一标识")] string workitemId,
        [Description("空间类型，默认为 Project")] string spaceType = "Project")
    {
        return await _apiClient.GetWorkItemInfoAsync(spaceIdentifier, spaceType, workitemId);
    }

    [McpServerTool, Description("搜索云效工作项列表。支持按关键字搜索，可按工作项类别筛选。返回匹配的工作项摘要列表。")]
    public async Task<string> SearchWorkItems(
        [Description("项目空间标识符（spaceIdentifier），通常是项目ID或项目路径")] string spaceIdentifier,
        [Description("搜索关键字，用于模糊匹配工作项标题")] string? keyword = null,
        [Description("工作项类别：Req（需求）、Bug（缺陷）、Task（任务），默认为 Req")] string? category = "Req")
    {
        return await _apiClient.SearchWorkItemsAsync(spaceIdentifier, keyword, category);
    }

    [McpServerTool, Description("获取当前企业下的项目列表。返回项目名称、项目ID、项目前缀等信息，可用于获取项目的 spaceIdentifier。")]
    public async Task<string> ListProjects()
    {
        return await _apiClient.ListProjectsAsync();
    }

    [McpServerTool, Description("列出云效 Flow 流水线（只读）。返回流水线 ID 与名称，可按名称和最近状态筛选；后续用 ListPipelineRuns/GetLatestPipelineRun 看运行情况。")]
    public async Task<string> ListPipelines(
        [Description("流水线名称模糊筛选，可空")] string? pipelineName = null,
        [Description("最近状态筛选，逗号分隔：SUCCESS,RUNNING,FAIL,CANCELED,WAITING，可空")] string? statusList = null,
        [Description("页码，从 1 开始")] int page = 1,
        [Description("每页条数，云效上限 30")] int perPage = 10)
    {
        return await _apiClient.ListPipelinesAsync(pipelineName, statusList, page, perPage);
    }

    [McpServerTool, Description("查询某条云效流水线的运行历史（只读）。参数：pipelineId（数字 ID）、可选 status 与 triggerMode 筛选。返回每次运行的 ID、状态、触发方式与起止时间。")]
    public async Task<string> ListPipelineRuns(
        [Description("流水线 ID（数字，先用 ListPipelines 查）")] string pipelineId,
        [Description("状态筛选 FAIL / SUCCESS / RUNNING，可空")] string? status = null,
        [Description("触发方式筛选：1 手动、2 定时、3 代码提交、5 流水线触发、6 Webhook，可空")] int? triggerMode = null,
        [Description("页码，从 1 开始")] int page = 1,
        [Description("每页条数，云效上限 30")] int perPage = 10)
    {
        return await _apiClient.ListPipelineRunsAsync(pipelineId, status, triggerMode, page, perPage);
    }

    [McpServerTool, Description("查看某次流水线运行的详情（只读）：状态、触发方式、代码源分支/仓库、每个阶段与任务的执行状态。参数：pipelineId, pipelineRunId。")]
    public async Task<string> GetPipelineRun(
        [Description("流水线 ID（数字）")] string pipelineId,
        [Description("运行实例 ID（数字，来自 ListPipelineRuns）")] string pipelineRunId)
    {
        return await _apiClient.GetPipelineRunAsync(pipelineId, pipelineRunId);
    }

    [McpServerTool, Description("查看流水线最近一次运行的详情（只读）。想知道“现在这条流水线挂没挂、卡在哪一步”，用这个最省事。参数：pipelineId。")]
    public async Task<string> GetLatestPipelineRun(
        [Description("流水线 ID（数字）")] string pipelineId)
    {
        return await _apiClient.GetLatestPipelineRunAsync(pipelineId);
    }
}
