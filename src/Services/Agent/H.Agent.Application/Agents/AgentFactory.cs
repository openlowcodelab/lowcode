using System;
using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// Agent 工厂 - 负责创建不同类型的 Agent 实例
/// </summary>
public class AgentFactory
{
    private readonly LLMProviderFactory _llmProviderFactory;
    
    public AgentFactory(LLMProviderFactory llmProviderFactory)
    {
        _llmProviderFactory = llmProviderFactory;
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
        
        return CreateAgentInstance(agentType, llmProvider);
    }
    
    /// <summary>
    /// 创建 Agent 实例（根据 ProviderName 选择模型，向后兼容）
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
        
        return CreateAgentInstance(agentType, llmProvider);
    }
    
    private static IAgentInstance? CreateAgentInstance(string agentType, ILLMProvider? llmProvider)
    {
        return agentType.ToLowerInvariant() switch
        {
            "general" => new GeneralAgent(llmProvider),
            "customer-service" => new CustomerServiceAgent(llmProvider),
            "data-analysis" => new DataAnalysisAgent(llmProvider),
            "automation-test" => new AutomationTestAgent(llmProvider),
            _ => new GeneralAgent(llmProvider)
        };
    }
    
    /// <summary>
    /// 获取所有可用的 Agent 类型
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
