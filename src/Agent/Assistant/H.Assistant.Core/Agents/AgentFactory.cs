using H.Assistant.Application.Contracts;
using H.Assistant.Core.Agents;
using H.Assistant.Core.Mcp;
using Microsoft.Extensions.Logging;

namespace H.Assistant.Core;

/// <summary>
/// Agent 工厂 - 创建 ReAct Agent 实例
/// </summary>
public class AgentFactory
{
    private readonly LLMProviderFactory _llmProviderFactory;
    private readonly IAgentAppService _agentDefinitionAppService;
    private readonly ISkillAppService _skillDefinitionAppService;
    private readonly IToolRegistry _toolRegistry;
    private readonly McpClientManager _mcpClientManager;
    private readonly ILogger<AgentFactory> _logger;
    private readonly ILogger<ReactAgent> _reactLogger;
    private readonly ILogger<ReactAgentInstance> _reactInstanceLogger;
    private readonly ILogger<ToolExecutor> _toolExecutorLogger;
    private bool _mcpInitialized;

    /// <summary>
    /// 内置默认智能体定义，当数据库中无已启用 Agent 时使用
    /// </summary>
    private static readonly AgentDto DefaultAgent = new()
    {
        Id = Guid.Empty,
        AgentType = "",
        DisplayName = "默认助手",
        Description = "通用智能助手，支持各类问答和任务",
        SystemPrompt = "你是一个具备推理和行动能力的智能助手。你可以使用各种工具来完成任务。\n" +
                       "当需要获取信息、执行操作或分析数据时，请主动使用合适的工具。\n" +
                       "请用简洁清晰的方式回答，并在需要时分步骤完成任务。",
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
        IToolRegistry toolRegistry,
        McpClientManager mcpClientManager,
        ILogger<AgentFactory> logger,
        ILogger<ReactAgent> reactLogger,
        ILogger<ReactAgentInstance> reactInstanceLogger,
        ILogger<ToolExecutor> toolExecutorLogger)
    {
        _llmProviderFactory = llmProviderFactory;
        _agentDefinitionAppService = agentDefinitionAppService;
        _skillDefinitionAppService = skillDefinitionAppService;
        _toolRegistry = toolRegistry;
        _mcpClientManager = mcpClientManager;
        _logger = logger;
        _reactLogger = reactLogger;
        _reactInstanceLogger = reactInstanceLogger;
        _toolExecutorLogger = toolExecutorLogger;
    }

    /// <summary>
    /// 确保 MCP 工具已初始化
    /// </summary>
    private async Task EnsureMcpInitializedAsync()
    {
        if (!_mcpInitialized)
        {
            try
            {
                await _mcpClientManager.InitializeAsync();
                // 将 MCP 工具注册到 ToolRegistry
                foreach (var tool in _mcpClientManager.GetAllTools())
                {
                    _toolRegistry.RegisterMcpTool(tool);
                }
                _mcpInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MCP Client 初始化失败，继续不使用 MCP 工具");
            }
        }
    }

    /// <summary>
    /// 创建 Agent 实例（根据 configId 选择模型）
    /// </summary>
    public async Task<IAgentInstance?> CreateAgentAsync(string agentType, Guid? modelConfigId)
    {
        var llmProvider = await ResolveProviderAsync(modelConfigId, null);
        if (llmProvider == null) return null;

        var definition = await ResolveAgentDefinitionAsync(agentType);
        return await BuildAgentInstanceAsync(llmProvider, definition);
    }

    /// <summary>
    /// 创建 Assistant 实例（根据 ProviderName 选择模型，向后兼容）
    /// </summary>
    public async Task<IAgentInstance?> CreateAgentAsync(string agentType, string? providerName = null)
    {
        var llmProvider = await ResolveProviderAsync(null, providerName);
        if (llmProvider == null) return null;

        var definition = await ResolveAgentDefinitionAsync(agentType);
        return await BuildAgentInstanceAsync(llmProvider, definition);
    }

    /// <summary>
    /// 解析 LLM Provider
    /// </summary>
    private async Task<ILLMProvider?> ResolveProviderAsync(Guid? modelConfigId, string? providerName)
    {
        ILLMProvider? llmProvider;

        if (modelConfigId.HasValue)
        {
            llmProvider = await _llmProviderFactory.CreateProviderAsync(modelConfigId.Value);
            if (llmProvider == null)
                _logger.LogWarning("无法创建 LLM Provider，configId={ConfigId}", modelConfigId.Value);
        }
        else if (!string.IsNullOrEmpty(providerName))
        {
            llmProvider = await _llmProviderFactory.CreateProviderAsync(providerName);
            if (llmProvider == null)
                _logger.LogWarning("无法创建 LLM Provider，providerName={ProviderName}", providerName);
        }
        else
        {
            llmProvider = await _llmProviderFactory.GetDefaultProviderAsync();
            if (llmProvider == null)
                _logger.LogWarning("无法获取默认 LLM Provider");
        }

        return llmProvider;
    }

    /// <summary>
    /// 解析 Agent 定义
    /// </summary>
    private async Task<AgentDto> ResolveAgentDefinitionAsync(string agentType)
    {
        var agents = (await _agentDefinitionAppService.GetEnabledAgentsAsync()).Data ?? [];
        var definition = agents.FirstOrDefault(a => a.AgentType == agentType);

        if (definition == null && string.IsNullOrEmpty(agentType))
        {
            definition = agents.FirstOrDefault();
            if (definition != null)
                _logger.LogInformation("AgentType 为空,使用默认 Agent: {AgentType}", definition.AgentType);
        }

        if (definition == null)
        {
            _logger.LogInformation("数据库中无已启用 Agent，使用内置默认智能体");
            definition = DefaultAgent;
        }

        return definition;
    }

    /// <summary>
    /// 构建 ReactAgentInstance
    /// </summary>
    private async Task<ReactAgentInstance> BuildAgentInstanceAsync(ILLMProvider llmProvider, AgentDto definition)
    {
        await EnsureMcpInitializedAsync();

        // 注册技能工具
        var skills = definition.Id != Guid.Empty
            ? (await _agentDefinitionAppService.GetAgentSkillsAsync(definition.Id)).Data ?? []
            : new List<SkillDto>();

        if (skills.Count > 0)
        {
            _toolRegistry.RegisterSkillTools(skills);
        }

        var toolDefs = _toolRegistry.GetToolDefinitions();
        var toolExecutor = new ToolExecutor(_toolRegistry, _toolExecutorLogger);

        _logger.LogInformation("创建 ReactAgent: {AgentName}, 可用工具数: {ToolCount}",
            definition.DisplayName, toolDefs.Count);

        return new ReactAgentInstance(
            llmProvider, definition, toolExecutor, toolDefs,
            _reactLogger, _reactInstanceLogger);
    }

    /// <summary>
    /// 获取所有可用的 Assistant 类型
    /// </summary>
    public async Task<List<AgentDefinition>> GetAvailableAgentsAsync()
    {
        var agents = new List<AgentDefinition>
        {
            new() {
                AgentType = "",
                DisplayName = "默认智能体",
                Description = "使用系统默认的 Agent 配置",
                Capabilities = []
            }
        };

        try
        {
            var dbAgents = (await _agentDefinitionAppService.GetEnabledAgentsAsync()).Data ?? [];
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
