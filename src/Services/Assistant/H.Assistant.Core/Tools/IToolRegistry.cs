using H.Assistant.Application.Contracts;
using Microsoft.Extensions.AI;

namespace H.Assistant.Core;

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
    /// 从技能定义注册工具
    /// </summary>
    void RegisterSkillTools(List<SkillDto> skills);
}
