using H.Assistant.Application.Contracts;
using Microsoft.Agents.AI;

namespace H.Assistant.Core;

/// <summary>
/// Agent 工厂 - 基于 Microsoft Agent Framework Agent
/// </summary>
public class AgentFactory
{
    private readonly LLMProviderFactory _llmProviderFactory;
    private readonly IAgentDefinitionAppService _agentDefinitionAppService;
    private readonly ISkillDefinitionAppService _skillDefinitionAppService;
    
    public AgentFactory(
        LLMProviderFactory llmProviderFactory,
        IAgentDefinitionAppService agentDefinitionAppService,
        ISkillDefinitionAppService skillDefinitionAppService)
    {
        _llmProviderFactory = llmProviderFactory;
        _agentDefinitionAppService = agentDefinitionAppService;
        _skillDefinitionAppService = skillDefinitionAppService;
    }
    
    /// <summary>
    /// 创建 Agent 实例（根据 configId 选择模型）
    /// </summary>
    public async Task<IAgentInstance?> CreateAgentAsync(string agentType, Guid? modelConfigId)
    {
        ILLMProvider? llmProvider;
        
        if (modelConfigId.HasValue)
        {
            llmProvider = await _llmProviderFactory.CreateProviderAsync(modelConfigId.Value);
        }
        else
        {
            llmProvider = await _llmProviderFactory.GetDefaultProviderAsync();
        }

        var agents = await _agentDefinitionAppService.GetEnabledAgentsAsync();
        var definition = agents.FirstOrDefault(a => a.AgentType == agentType);

        if (definition == null)
        {
            return null;
        }

        // 使用 Microsoft Agent Framework 创建 Agent
        var skills = await _agentDefinitionAppService.GetAgentSkillsAsync(definition.Id);
        return new FrameworkAgentInstance(llmProvider, definition, skills);
    }
    
    /// <summary>
    /// 创建 Assistant 实例（根据 ProviderName 选择模型，向后兼容）
    /// </summary>
    public async Task<IAgentInstance?> CreateAgentAsync(string agentType, string? providerName = null)
    {
        ILLMProvider? llmProvider;
        
        if (!string.IsNullOrEmpty(providerName))
        {
            llmProvider = await _llmProviderFactory.CreateProviderAsync(providerName);
        }
        else
        {
            llmProvider = await _llmProviderFactory.GetDefaultProviderAsync();
        }

        var agents = await _agentDefinitionAppService.GetEnabledAgentsAsync();
        var definition = agents.FirstOrDefault(a => a.AgentType == agentType);

        if (definition == null)
        {
            return null;
        }

        // 使用 Microsoft Agent Framework 创建 Agent
        var skills = await _agentDefinitionAppService.GetAgentSkillsAsync(definition.Id);
        return new FrameworkAgentInstance(llmProvider, definition, skills);
    }
    
    /// <summary>
    /// 获取所有可用的 Assistant 类型
    /// 合并数据库定义和硬编码定义
    /// </summary>
    public async Task<List<AgentDefinition>> GetAvailableAgentsAsync()
    {
        var agents = new List<AgentDefinition>();
        
        // 从数据库加载启用的 Agent
        if (_agentDefinitionAppService != null)
        {
            try
            {
                var dbAgents = await _agentDefinitionAppService.GetEnabledAgentsAsync();
                agents.AddRange(dbAgents.Select(a => new AgentDefinition
                {
                    AgentType = a.AgentType,
                    DisplayName = a.DisplayName,
                    Description = a.Description,
                    Capabilities = a.Skills
                }));
            }
            catch
            {
                // 如果数据库查询失败，使用硬编码定义
            }
        }
        
        // 如果没有数据库定义，使用硬编码定义
        if (!agents.Any())
        {
            agents.AddRange(GetAvailableAgents());
        }
        
        return agents;
    }
    
    /// <summary>
    /// 获取所有可用的 Assistant 类型（硬编码，向后兼容）
    /// </summary>
    public List<AgentDefinition> GetAvailableAgents()
    {
        return new List<AgentDefinition>
        {
            new AgentDefinition
            {
                AgentType = "general",
                DisplayName = "通用助手",
                Description = "通用智能助手，支持问答、文本生成、代码编写等常见任务",
                Capabilities = new List<string> { "问答", "文本生成", "代码编写", "翻译", "总结" }
            },
            new AgentDefinition
            {
                AgentType = "customer-service",
                DisplayName = "客服助手",
                Description = "智能客服助手，支持问题解答、工单处理、客户反馈等客服场景",
                Capabilities = new List<string> { "问题解答", "工单处理", "客户反馈", "知识库查询" }
            },
            new AgentDefinition
            {
                AgentType = "data-analysis",
                DisplayName = "数据分析助手",
                Description = "数据分析智能助手，支持数据查询、统计分析、报表生成等",
                Capabilities = new List<string> { "数据查询", "统计分析", "报表生成", "趋势预测" }
            },
            new AgentDefinition
            {
                AgentType = "automation-test",
                DisplayName = "自动化测试助手",
                Description = "自动化测试智能助手，支持测试用例生成、测试执行、结果分析等",
                Capabilities = new List<string> { "测试用例生成", "测试执行", "结果分析", "缺陷报告" }
            }
        };
    }
}
