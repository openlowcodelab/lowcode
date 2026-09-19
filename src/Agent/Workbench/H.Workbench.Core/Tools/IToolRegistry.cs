using H.Workbench.Application.Contracts;
using Microsoft.Extensions.AI;

namespace H.Workbench.Core;

/// <summary>
/// 工具注册中心接口
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// 获取所有已注册的工具
    /// </summary>
    IReadOnlyList<AIFunction> GetAllTools();

    /// <summary>
    /// 按名称获取工具
    /// </summary>
    AIFunction? GetTool(string name);

    /// <summary>
    /// 获取所有工具的 OpenAI 格式定义（用于 LLM API 请求的 tools 参数）
    /// </summary>
    List<ToolDefinition> GetToolDefinitions();

    /// <summary>
    /// 注册 MCP 工具
    /// </summary>
    void RegisterMcpTool(AIFunction tool);

    /// <summary>
    /// 从技能定义注册工具，返回成功/失败统计
    /// </summary>
    SkillRegistrationReport RegisterSkillTools(List<SkillDto> skills);

    /// <summary>
    /// 创建按归属技能过滤的工具视图；allowedOwners 为 null 表示全量视图（不过滤）
    /// </summary>
    IToolRegistry CreateScoped(IReadOnlyCollection<string>? allowedOwners);
}

/// <summary>
/// 技能工具注册结果
/// </summary>
public sealed record SkillRegistrationReport(int Ok, int Failed, IReadOnlyList<string> Errors);
