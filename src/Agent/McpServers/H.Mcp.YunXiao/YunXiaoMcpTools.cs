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
}
