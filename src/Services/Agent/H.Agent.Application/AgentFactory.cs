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
    /// 创建 Agent 实例
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

/// <summary>
/// Agent 定义
/// </summary>
public class AgentDefinition
{
    public string AgentType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
}

/// <summary>
/// Agent 实例接口
/// </summary>
public interface IAgentInstance
{
    string Name { get; }
    string SystemPrompt { get; }
    Task<string> ProcessMessageAsync(string message, List<string>? conversationHistory = null);
    List<string> GetAvailableTools();
}

/// <summary>
/// Agent 基类 - 提供 LLM 调用能力
/// </summary>
public abstract class AgentBase : IAgentInstance
{
    protected readonly ILLMProvider? _llmProvider;
    
    protected AgentBase(ILLMProvider? llmProvider)
    {
        _llmProvider = llmProvider;
    }
    
    public abstract string Name { get; }
    public abstract string SystemPrompt { get; }
    public abstract List<string> GetAvailableTools();
    
    public virtual async Task<string> ProcessMessageAsync(string message, List<string>? conversationHistory = null)
    {
        if (_llmProvider == null)
        {
            return "[系统] 未配置 LLM Provider，请在模型配置页面添加并启用 Provider。";
        }
        
        try
        {
            var messages = new List<Message>
            {
                new Message { Role = "system", Content = SystemPrompt }
            };
            
            if (conversationHistory != null)
            {
                foreach (var hist in conversationHistory)
                {
                    var parts = hist.Split(':', 2);
                    if (parts.Length == 2)
                    {
                        messages.Add(new Message
                        {
                            Role = parts[0].Trim().ToLower() == "user" ? "user" : "assistant",
                            Content = parts[1].Trim()
                        });
                    }
                }
            }
            
            messages.Add(new Message { Role = "user", Content = message });
            
            var request = new LLMRequest
            {
                Messages = messages,
                Temperature = 0.7f,
                MaxTokens = 2000
            };
            
            var response = await _llmProvider.ChatAsync(request);
            return response.Content;
        }
        catch (Exception ex)
        {
            return $"[错误] 调用 LLM 失败：{ex.Message}";
        }
    }
}

/// <summary>
/// 通用 Agent 实现
/// </summary>
public class GeneralAgent : AgentBase
{
    public override string Name => "通用助手";
    
    public override string SystemPrompt => """
        你是一个通用的智能助手，能够帮助用户解答问题、生成文本、编写代码、翻译语言等。
        请用简洁语言回答用户的问题，保持友好和专业的态度。
        """;
    
    public GeneralAgent(ILLMProvider? llmProvider) : base(llmProvider) { }
    
    public override List<string> GetAvailableTools()
    {
        return new List<string> { "代码执行", "文本翻译", "摘要生成", "知识查询" };
    }
}

/// <summary>
/// 客服 Agent 实现
/// </summary>
public class CustomerServiceAgent : AgentBase
{
    public override string Name => "客服助手";
    
    public override string SystemPrompt => """
        你是一个专业的智能客服助手，能够帮助客户解答问题、处理工单和收集反馈。
        请根据知识库提供准确的回答，并在需要时建议创建工单。
        """;
    
    public CustomerServiceAgent(ILLMProvider? llmProvider) : base(llmProvider) { }
    
    public override List<string> GetAvailableTools()
    {
        return new List<string> { "知识库查询", "工单创建", "客户反馈", "常见问题解答" };
    }
}

/// <summary>
/// 数据分析 Agent 实现
/// </summary>
public class DataAnalysisAgent : AgentBase
{
    public override string Name => "数据分析助手";
    
    public override string SystemPrompt => """
        你是一个数据分析智能助手，能够帮助用户进行数据查询、统计分析和报表生成。
        请提供清晰的数据分析结果，并给出专业的解读建议。
        """;
    
    public DataAnalysisAgent(ILLMProvider? llmProvider) : base(llmProvider) { }
    
    public override List<string> GetAvailableTools()
    {
        return new List<string> { "SQL 查询", "统计分析", "图表生成", "趋势预测", "报表导出" };
    }
}

/// <summary>
/// 自动化测试 Agent 实现
/// </summary>
public class AutomationTestAgent : AgentBase
{
    public override string Name => "自动化测试助手";
    
    public override string SystemPrompt => """
        你是一个自动化测试智能助手，能够帮助用户生成测试用例、执行测试并分析结果。
        请提供专业的测试建议和详细的测试报告。
        """;
    
    public AutomationTestAgent(ILLMProvider? llmProvider) : base(llmProvider) { }
    
    public override List<string> GetAvailableTools()
    {
        return new List<string> { "测试用例生成", "Playwright 测试", "API 测试", "结果分析", "缺陷报告" };
    }
}
