using H.Assistant.Application.Contracts;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace H.Assistant.Core;

/// <summary>
/// Agent 工厂 - 基于 Microsoft Agent Framework Agent
/// </summary>
public class AgentFactory
{
    private readonly LLMProviderFactory _llmProviderFactory;
    private readonly IAgentAppService _agentDefinitionAppService;
    private readonly ISkillAppService _skillDefinitionAppService;
    private readonly ILogger<AgentFactory> _logger;
    
    /// <summary>
    /// 内置默认智能体定义，当数据库中无已启用 Agent 时使用
    /// </summary>
    private static readonly AgentDto DefaultAgent = new()
    {
        Id = Guid.Empty,
        AgentType = "",
        DisplayName = "默认助手",
        Description = "通用智能助手，支持各类问答和任务",
        SystemPrompt = "你是一个智能助手，请用简洁清晰的方式回答用户的问题。",
        IsEnabled = true,
        SupportsStreaming = true,
        Temperature = 0.7f,
        MaxTokens = 2000,
        Skills = new List<string>()
    };
    
    public AgentFactory(
        LLMProviderFactory llmProviderFactory,
        IAgentAppService agentDefinitionAppService,
        ISkillAppService skillDefinitionAppService,
        ILogger<AgentFactory> logger)
    {
        _llmProviderFactory = llmProviderFactory;
        _agentDefinitionAppService = agentDefinitionAppService;
        _skillDefinitionAppService = skillDefinitionAppService;
        _logger = logger;
    }
    
    /// <summary>
    /// 创建 Agent 实例（根据 configId 选择模型）
    /// </summary>
    public async Task<IAgentInstance?> CreateAgentAsync(string agentType, Guid? modelConfigId)
    {
        ILLMProvider? llmProvider;
        bool llmProviderFailed = false;
        
        if (modelConfigId.HasValue)
        {
            llmProvider = await _llmProviderFactory.CreateProviderAsync(modelConfigId.Value);
            if (llmProvider == null)
            {
                _logger.LogWarning("无法创建 LLM Provider，configId={ConfigId}。请检查该配置是否存在、已启用且 API Key 不为空", modelConfigId.Value);
                llmProviderFailed = true;
            }
        }
        else
        {
            llmProvider = await _llmProviderFactory.GetDefaultProviderAsync();
            if (llmProvider == null)
            {
                _logger.LogWarning("无法获取默认 LLM Provider。请检查是否存在 IsDefault=true 且 IsEnabled=true 且 ApiKey 不为空的配置");
                llmProviderFailed = true;
            }
        }

        if (llmProviderFailed)
        {
            return null;
        }

        var agents = await _agentDefinitionAppService.GetEnabledAgentsAsync();
        var definition = agents.FirstOrDefault(a => a.AgentType == agentType);

        // 如果 agentType 为空或未找到,使用第一个启用的 Agent
        if (definition == null && string.IsNullOrEmpty(agentType))
        {
            definition = agents.FirstOrDefault();
            if (definition != null)
            {
                _logger.LogInformation("AgentType 为空,使用默认 Agent: {AgentType}", definition.AgentType);
            }
        }

        if (definition == null)
        {
            _logger.LogInformation("数据库中无已启用 Agent，使用内置默认智能体");
            definition = DefaultAgent;
        }

        // 使用 Microsoft Agent Framework 创建 Agent
        var skills = definition.Id != Guid.Empty
            ? await _agentDefinitionAppService.GetAgentSkillsAsync(definition.Id)
            : new List<SkillDto>();
        return new FrameworkAgentInstance(llmProvider!, definition, skills);
    }
    
    /// <summary>
    /// 创建 Assistant 实例（根据 ProviderName 选择模型，向后兼容）
    /// </summary>
    public async Task<IAgentInstance?> CreateAgentAsync(string agentType, string? providerName = null)
    {
        ILLMProvider? llmProvider;
        bool llmProviderFailed = false;
        
        if (!string.IsNullOrEmpty(providerName))
        {
            llmProvider = await _llmProviderFactory.CreateProviderAsync(providerName);
            if (llmProvider == null)
            {
                _logger.LogWarning("无法创建 LLM Provider，providerName={ProviderName}。请检查该配置是否存在、已启用且 API Key 不为空", providerName);
                llmProviderFailed = true;
            }
        }
        else
        {
            llmProvider = await _llmProviderFactory.GetDefaultProviderAsync();
            if (llmProvider == null)
            {
                _logger.LogWarning("无法获取默认 LLM Provider。请检查是否存在 IsDefault=true 且 IsEnabled=true 且 ApiKey 不为空的配置");
                llmProviderFailed = true;
            }
        }

        if (llmProviderFailed)
        {
            return null;
        }

        var agents = await _agentDefinitionAppService.GetEnabledAgentsAsync();
        var definition = agents.FirstOrDefault(a => a.AgentType == agentType);

        // 如果 agentType 为空或未找到,使用第一个启用的 Agent
        if (definition == null && string.IsNullOrEmpty(agentType))
        {
            definition = agents.FirstOrDefault();
            if (definition != null)
            {
                _logger.LogInformation("AgentType 为空,使用默认 Agent: {AgentType}", definition.AgentType);
            }
        }

        if (definition == null)
        {
            _logger.LogInformation("数据库中无已启用 Agent，使用内置默认智能体");
            definition = DefaultAgent;
        }

        // 使用 Microsoft Agent Framework 创建 Agent
        var skills = definition.Id != Guid.Empty
            ? await _agentDefinitionAppService.GetAgentSkillsAsync(definition.Id)
            : new List<SkillDto>();
        return new FrameworkAgentInstance(llmProvider!, definition, skills);
    }

    /// <summary>
    /// 获取所有可用的 Assistant 类型
    /// 只返回数据库中启用的 Agent,并在首位添加"默认"选项
    /// </summary>
    public async Task<List<AgentDefinition>> GetAvailableAgentsAsync()
    {
        var agents = new List<AgentDefinition>
        {
            // 添加"默认"选项
            new() {
                AgentType = "",
                DisplayName = "默认智能体",
                Description = "使用系统默认的 Agent 配置",
                Capabilities = []
            }
        };

        // 从数据库加载启用的 Agent
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
            // 如果数据库查询失败，返回空列表
        }

        return agents;
    }
}
