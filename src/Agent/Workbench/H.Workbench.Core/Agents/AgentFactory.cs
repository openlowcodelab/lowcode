using H.Workbench.Application.Contracts;
using H.Workbench.Core.Agents;
using H.Workbench.Core.Mcp;
using H.Workbench.Core.Tools;
using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace H.Workbench.Core;

/// <summary>
/// Agent 工厂 - 创建 ReAct Agent 实例
/// </summary>
public class AgentFactory
{
    /// <summary>
    /// 运行时上下文注入提示词的硬上限（字符）
    /// </summary>
    private const int RuntimeContextMaxChars = 6000;

    private readonly LLMProviderFactory _llmProviderFactory;
    private readonly IAgentAppService _agentDefinitionAppService;
    private readonly ISkillAppService _skillDefinitionAppService;
    private readonly IWorkbenchProjectAppService _projectAppService;
    private readonly IKnowledgeRetrievalAppService _knowledgeRetrievalAppService;
    private readonly IToolRegistry _toolRegistry;
    private readonly McpClientManager _mcpClientManager;
    private readonly WorkbenchToolOptions _options;
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
        IWorkbenchProjectAppService projectAppService,
        IKnowledgeRetrievalAppService knowledgeRetrievalAppService,
        IToolRegistry toolRegistry,
        McpClientManager mcpClientManager,
        IOptions<WorkbenchToolOptions> options,
        ILogger<AgentFactory> logger,
        ILogger<ReactAgent> reactLogger,
        ILogger<ReactAgentInstance> reactInstanceLogger,
        ILogger<ToolExecutor> toolExecutorLogger)
    {
        _llmProviderFactory = llmProviderFactory;
        _agentDefinitionAppService = agentDefinitionAppService;
        _skillDefinitionAppService = skillDefinitionAppService;
        _projectAppService = projectAppService;
        _knowledgeRetrievalAppService = knowledgeRetrievalAppService;
        _toolRegistry = toolRegistry;
        _mcpClientManager = mcpClientManager;
        _options = options.Value;
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
    /// 创建 Agent 实例（根据 configId 选择模型）。
    /// 模型解析优先级：调用方传入 > 员工默认模型 > ProviderName > 全局默认。
    /// </summary>
    public async Task<IAgentInstance?> CreateAgentAsync(string agentType, Guid? modelConfigId,
        AgentRunContext? runContext = null)
    {
        var definition = await ResolveAgentDefinitionAsync(agentType);
        var llmProvider = await ResolveProviderAsync(
            modelConfigId, providerName: null, agentDefaultId: definition.DefaultModelConfigId);
        if (llmProvider == null) return null;

        return await BuildAgentInstanceAsync(llmProvider, definition, runContext);
    }

    /// <summary>
    /// 创建 Workbench 实例（根据 ProviderName 选择模型，向后兼容）
    /// </summary>
    public async Task<IAgentInstance?> CreateAgentAsync(string agentType, string? providerName = null)
    {
        var definition = await ResolveAgentDefinitionAsync(agentType);
        var llmProvider = await ResolveProviderAsync(
            null, providerName, agentDefaultId: definition.DefaultModelConfigId);
        if (llmProvider == null) return null;

        return await BuildAgentInstanceAsync(llmProvider, definition);
    }

    /// <summary>
    /// 解析 LLM Provider：逐档回落，每档失败只记 Warning 不中断
    /// </summary>
    private async Task<ILLMProvider?> ResolveProviderAsync(Guid? modelConfigId, string? providerName, Guid? agentDefaultId)
    {
        if (modelConfigId.HasValue)
        {
            var provider = await _llmProviderFactory.CreateProviderAsync(modelConfigId.Value);
            if (provider != null) return provider;
            _logger.LogWarning("指定模型配置不可用（configId={ConfigId}），尝试回落", modelConfigId.Value);
        }

        if (agentDefaultId.HasValue && agentDefaultId.Value != modelConfigId)
        {
            var provider = await _llmProviderFactory.CreateProviderAsync(agentDefaultId.Value);
            if (provider != null) return provider;
            _logger.LogWarning("员工默认模型不可用（configId={ConfigId}），尝试回落全局默认", agentDefaultId.Value);
        }

        if (!string.IsNullOrEmpty(providerName))
        {
            var provider = await _llmProviderFactory.CreateProviderAsync(providerName);
            if (provider != null) return provider;
            _logger.LogWarning("无法创建 LLM Provider，providerName={ProviderName}", providerName);
        }

        var fallback = await _llmProviderFactory.GetDefaultProviderAsync();
        if (fallback == null)
            _logger.LogWarning("无法获取默认 LLM Provider");
        return fallback;
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
    private async Task<ReactAgentInstance> BuildAgentInstanceAsync(
        ILLMProvider llmProvider, AgentDto definition, AgentRunContext? runContext = null)
    {
        await EnsureMcpInitializedAsync();

        // 员工绑定的技能（决定工具可见性与提示词上下文）
        var agentSkills = definition.Id != Guid.Empty
            ? (await _agentDefinitionAppService.GetAgentSkillsAsync(definition.Id)).Data ?? []
            : new List<SkillDto>();

        // 全部启用技能注册到全局注册表（带归属登记），随后按员工技能过滤可见性
        var enabledSkills = (await _skillDefinitionAppService.GetEnabledSkillsAsync()).Data ?? [];
        var report = _toolRegistry.RegisterSkillTools(enabledSkills);
        if (report.Failed > 0)
        {
            foreach (var error in report.Errors) _logger.LogWarning("技能注册失败: {Error}", error);
        }

        var scopedRegistry = ResolveScopedRegistry(definition, agentSkills);
        var toolDefs = scopedRegistry.GetToolDefinitions();
        var toolExecutor = new ToolExecutor(scopedRegistry, _toolExecutorLogger, _options.ToolTimeoutSeconds);

        _logger.LogInformation("创建 ReactAgent: {AgentName}, 可用工具数: {ToolCount}{ScopeNote}",
            definition.DisplayName, toolDefs.Count,
            ReferenceEquals(scopedRegistry, _toolRegistry) ? "（全量，未隔离）" : "（按员工技能隔离）");

        // 运行时上下文注入（知识库检索 + 项目仓库清单）
        Func<string, Task<string>>? augmentor = null;
        var hasContext = definition.KnowledgeBaseIds.Count > 0
                         || definition.ProjectIds.Count > 0
                         || runContext?.ProjectId != null;
        if (hasContext)
        {
            _logger.LogInformation("运行时上下文注入启用: kb={KbCount}, project={ProjCount}",
                definition.KnowledgeBaseIds.Count, definition.ProjectIds.Count);
            augmentor = async q => await BuildRuntimeContextAsync(definition, runContext, q);
        }

        return new ReactAgentInstance(
            llmProvider, definition, toolExecutor, toolDefs,
            _reactLogger, _reactInstanceLogger, augmentor);
    }

    /// <summary>
    /// 员工级工具隔离：绑定了技能的员工只见自己的技能工具 + MCP；
    /// 未绑定技能的员工保持全量（避免存量员工失能），可用 Workbench:ToolIsolationEnabled 一键回退。
    /// </summary>
    private IToolRegistry ResolveScopedRegistry(AgentDto definition, List<SkillDto> agentSkills)
    {
        if (!_options.ToolIsolationEnabled || definition.SkillIdList.Count == 0)
            return _toolRegistry;

        var allowed = agentSkills
            .Where(s => s.IsEnabled && s.SkillType != "Planned")
            .Select(s => s.SkillName)
            .ToList();
        allowed.Add(WorkbenchToolCatalog.McpOwner);

        return _toolRegistry.CreateScoped(allowed);
    }

    /// <summary>
    /// 拼装注入 SystemPrompt 的运行时上下文：员工绑定项目的仓库清单 + 知识库检索片段
    /// </summary>
    private async Task<string> BuildRuntimeContextAsync(AgentDto definition, AgentRunContext? runContext, string userMessage)
    {
        var sb = new StringBuilder();
        var withRepoCount = 0;
        var snippetCount = 0;

        // 1) 可操作仓库清单（员工绑定的项目 + 任务所属项目）
        var projectIds = definition.ProjectIds.ToList();
        if (runContext?.ProjectId != null && !projectIds.Contains(runContext.ProjectId.Value))
            projectIds.Add(runContext.ProjectId.Value);

        if (projectIds.Count > 0)
        {
            var projects = (await _projectAppService.GetByIdsAsync(projectIds)).Data ?? [];
            var withRepo = projects.Where(p => !string.IsNullOrWhiteSpace(p.RepoUrl)).Take(5).ToList();
            withRepoCount = withRepo.Count;
            if (withRepo.Count > 0)
            {
                sb.AppendLine("## 可操作的代码仓库（仅限以下仓库，禁止操作清单外的地址）");
                foreach (var p in withRepo)
                {
                    var url = p.RepoUrl!.Trim();
                    var dir = GitWorkspaceResolver.DeriveDirName(url);
                    sb.AppendLine($"- {p.ProjectName} | 地址: {url} | 默认分支: {(string.IsNullOrWhiteSpace(p.DefaultBranch) ? "(远端默认)" : p.DefaultBranch)} | 本地目录: {dir}");
                }
                sb.AppendLine("克隆时用上述\"本地目录\"作为 dirName；repo 参数传目录名即可。");
            }
        }

        // 2) 知识库检索片段（以当前问题为查询）
        if (definition.KnowledgeBaseIds.Count > 0 && !string.IsNullOrWhiteSpace(userMessage))
        {
            var search = await _knowledgeRetrievalAppService.SearchAsync(new SearchKnowledgeInput
            {
                KnowledgeBaseIds = definition.KnowledgeBaseIds,
                Query = userMessage,
                TopN = 4
            });
            var snippets = search.Data ?? [];
            snippetCount = snippets.Count;
            if (snippets.Count > 0)
            {
                sb.AppendLine("## 参考资料（来自员工绑定的知识库，按关键词检索）");
                for (var i = 0; i < snippets.Count; i++)
                {
                    var s = snippets[i];
                    sb.AppendLine($"### [{i + 1}] {s.Title}（知识库：{s.KnowledgeBaseName}）");
                    sb.AppendLine(s.Snippet);
                }
                sb.AppendLine("回答时优先依据上述参考资料；与问题无关时可忽略。");
            }
        }

        var result = sb.ToString().TrimEnd();
        _logger.LogInformation("运行时上下文构建完成: {Len} 字符（仓库 {Repos} 条 / 知识 {Snips} 段）",
            result.Length, withRepoCount, snippetCount);
        if (result.Length > RuntimeContextMaxChars)
        {
            _logger.LogWarning("运行时上下文超长（{Len}），已截断", result.Length);
            result = result[..RuntimeContextMaxChars];
        }
        return result;
    }

    /// <summary>
    /// 获取所有可用的 Workbench 类型
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

/// <summary>
/// 单次运行的上下文（任务级项目归属等）
/// </summary>
public sealed record AgentRunContext(Guid? ProjectId = null, string? ExtraInstruction = null);
