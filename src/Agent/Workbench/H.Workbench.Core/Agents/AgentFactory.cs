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
    private readonly ApprovalGateway _approvalGateway;
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
        ApprovalGateway approvalGateway,
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
        _approvalGateway = approvalGateway;
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

        // 工具名 → 归属技能；需审批工具集 = 技能 RequiresApproval 命中者（员工隔离视图内计算）
        var toolOwners = toolDefs
            .Select(d => d.Function.Name)
            .Where(n => scopedRegistry.GetToolOwner(n) is not null)
            .ToDictionary(n => n, n => scopedRegistry.GetToolOwner(n)!, StringComparer.OrdinalIgnoreCase);
        var approval = BuildApprovalContext(runContext, scopedRegistry, enabledSkills, toolOwners);

        _logger.LogInformation("创建 ReactAgent: {AgentName}, 可用工具数: {ToolCount}{ScopeNote}{ApprovalNote}",
            definition.DisplayName, toolDefs.Count,
            ReferenceEquals(scopedRegistry, _toolRegistry) ? "（全量，未隔离）" : "（按员工技能隔离）",
            approval is null ? "" : $"（审批模式 {approval.Mode}，需审批工具 {approval.ApprovalTools.Count} 个）");

        // 运行时上下文注入（知识库检索 + 项目仓库清单 + 历史经验记忆）
        Func<string, Task<string>>? augmentor = null;
        var hasContext = definition.KnowledgeBaseIds.Count > 0
                         || definition.ProjectIds.Count > 0
                         || runContext?.ProjectId != null
                         || _options.Memory.Enabled;
        if (hasContext)
        {
            _logger.LogInformation("运行时上下文注入启用: kb={KbCount}, project={ProjCount}",
                definition.KnowledgeBaseIds.Count, definition.ProjectIds.Count);
            augmentor = async q => await BuildRuntimeContextAsync(definition, runContext, q);
        }

        return new ReactAgentInstance(
            llmProvider, definition, toolExecutor, toolDefs,
            _reactLogger, _reactInstanceLogger, augmentor, toolOwners, approval);
    }

    /// <summary>
    /// 审批门上下文：仅当调用方声明了审批模式（Task 路径）且全局开关开启时构建；
    /// MCP 工具无技能归属默认免审批（本轮不做 server 级授权）。
    /// </summary>
    private AgentApprovalContext? BuildApprovalContext(
        AgentRunContext? runContext,
        IToolRegistry scopedRegistry,
        List<SkillDto> enabledSkills,
        Dictionary<string, string> toolOwners)
    {
        var mode = runContext?.ApprovalMode;
        if (string.IsNullOrEmpty(mode) || mode == "None" || !_options.Approval.Enabled)
        {
            return null;
        }

        var skillApproval = enabledSkills
            .Where(s => s.IsEnabled && s.RequiresApproval)
            .Select(s => s.SkillName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var approvalTools = toolOwners
            .Where(kv => skillApproval.Contains(kv.Value))
            .Select(kv => kv.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new AgentApprovalContext(
            _approvalGateway,
            approvalTools,
            mode,
            _options.Approval.TimeoutSeconds,
            _options.Approval.MaxPerExecution,
            string.Equals(_options.Approval.NonInteractivePolicy, "Allow", StringComparison.OrdinalIgnoreCase),
            runContext.TaskId ?? Guid.Empty,
            runContext.TaskLogId ?? Guid.Empty,
            runContext.UserId);
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
    /// 资源类型的中文标签（供提示词阅读）
    /// </summary>
    private static string ResourceTypeLabel(string type) => type switch
    {
        WorkbenchResourceTypes.Code => "代码仓库",
        WorkbenchResourceTypes.Design => "设计文件",
        WorkbenchResourceTypes.Document => "文档",
        WorkbenchResourceTypes.Data => "数据集",
        _ => "其他资源"
    };

    /// <summary>
    /// 拼装注入 SystemPrompt 的运行时上下文：当前项目档案（描述+资源）+ 知识库检索片段
    /// </summary>
    private async Task<string> BuildRuntimeContextAsync(AgentDto definition, AgentRunContext? runContext, string userMessage)
    {
        var sb = new StringBuilder();
        var projectCount = 0;
        var repoCount = 0;

        // 项目档案：本次任务所属项目置顶，其后是员工绑定的项目
        var runProjectId = runContext?.ProjectId;
        var projectIds = new List<Guid>();
        if (runProjectId.HasValue) projectIds.Add(runProjectId.Value);
        projectIds.AddRange(definition.ProjectIds.Where(id => id != runProjectId));
        projectIds = projectIds.Distinct().Take(5).ToList();

        var projects = projectIds.Count > 0
            ? (await _projectAppService.GetByIdsAsync(projectIds)).Data ?? []
            : [];
        // 按 projectIds 顺序输出，任务项目置顶；已删除的项目自然落空
        var ordered = projectIds
            .Select(id => projects.FirstOrDefault(p => p.Id == id))
            .Where(p => p is not null)
            .Select(p => p!)
            .ToList();

        if (ordered.Count == 0)
        {
            sb.AppendLine("## 当前项目上下文");
            sb.AppendLine("本次任务未关联项目，员工也未绑定任何项目；请勿虚构项目信息，可说明当前无项目上下文。");
            sb.AppendLine();
            return await AppendKnowledgeSnippetsAsync(sb, definition, userMessage, 0, 0);
        }

        projectCount = ordered.Count;
        sb.AppendLine("## 当前项目上下文（系统内已登记的项目信息，回答项目相关问题时直接依据，勿再向用户索要）");
        foreach (var project in ordered)
        {
            var isRunProject = runProjectId.HasValue && project.Id == runProjectId.Value;
            sb.AppendLine($"### 项目：{project.ProjectName}{(isRunProject ? "（本次任务所属项目）" : "（员工关联项目）")}");
            sb.AppendLine(string.IsNullOrWhiteSpace(project.Description)
                ? "描述: （未填写）"
                : $"描述: {project.Description.Trim()}");

            if (project.Resources.Count == 0)
            {
                sb.AppendLine("关联资源: （无）");
                sb.AppendLine();
                continue;
            }

            sb.AppendLine("关联资源:");
            foreach (var resource in project.Resources.Take(10))
            {
                var line = new StringBuilder($"- {ResourceTypeLabel(resource.ResourceType)}：{resource.Name}");
                var url = resource.Url?.Trim();
                if (!string.IsNullOrEmpty(url)) line.Append($" | 地址: {url}");

                if (resource.ResourceType == WorkbenchResourceTypes.Code)
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        line.Append(" | 未配置仓库地址，不可操作");
                    }
                    else
                    {
                        line.Append($" | 默认分支: {(string.IsNullOrWhiteSpace(resource.Branch) ? "(远端默认)" : resource.Branch)} | 本地目录: {GitWorkspaceResolver.DeriveDirName(url)}");
                        repoCount++;
                    }
                }

                sb.AppendLine(line.ToString());
            }
            sb.AppendLine();
        }

        sb.AppendLine(repoCount > 0
            ? "上述\"代码仓库\"即允许操作的仓库清单，禁止操作清单外的地址；克隆时用\"本地目录\"作为 dirName，repo 参数传目录名即可。"
            : "当前项目未配置代码仓库；涉及代码操作时请告知用户需先在项目资源中登记仓库地址。");
        sb.AppendLine();

        return await AppendKnowledgeSnippetsAsync(sb, definition, userMessage, projectCount, repoCount);
    }

    /// <summary>
    /// 追加知识库检索片段并收尾（超长截断），返回最终注入 SystemPrompt 的上下文字符串
    /// </summary>
    private async Task<string> AppendKnowledgeSnippetsAsync(
        StringBuilder sb, AgentDto definition, string userMessage, int projectCount, int repoCount)
    {
        var snippetCount = await AppendKnowledgeSnippetsCoreAsync(sb, definition, userMessage);

        // 历史经验记忆：跨会话自动抽取的经验条目按当前问题检索回填（阶段C学习闭环的"读"侧）
        var memoryCount = 0;
        if (_options.Memory.Enabled && !string.IsNullOrWhiteSpace(userMessage))
        {
            try
            {
                var memories = await _knowledgeRetrievalAppService.SearchMemoryAsync(new SearchMemoryInput
                {
                    Query = userMessage,
                    TopN = _options.Memory.TopN,
                    SnippetMaxChars = _options.Memory.MaxCharsPerMemory
                });
                var items = memories.Data ?? [];
                memoryCount = items.Count;
                if (items.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("## 历史经验（你过去会话中记住的要点，可信度低于知识库资料，与当前事实冲突时以当前为准）");
                    foreach (var m in items)
                    {
                        sb.AppendLine($"- [{m.KnowledgeBaseName}] {m.Title}：{m.Snippet}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "记忆检索失败，跳过注入");
            }
        }

        var result = sb.ToString().TrimEnd();
        _logger.LogInformation("运行时上下文构建完成: {Len} 字符（项目 {Projects} 个 / 仓库 {Repos} 条 / 知识 {Snips} 段 / 记忆 {Mems} 条）",
            result.Length, projectCount, repoCount, snippetCount, memoryCount);
        if (result.Length > RuntimeContextMaxChars)
        {
            _logger.LogWarning("运行时上下文超长（{Len}），已截断", result.Length);
            result = result[..RuntimeContextMaxChars];
        }
        return result;
    }

    /// <summary>
    /// 追加知识库检索片段，返回注入段数
    /// </summary>
    private async Task<int> AppendKnowledgeSnippetsCoreAsync(
        StringBuilder sb, AgentDto definition, string userMessage)
    {
        if (definition.KnowledgeBaseIds.Count == 0 || string.IsNullOrWhiteSpace(userMessage))
        {
            return 0;
        }

        var search = await _knowledgeRetrievalAppService.SearchAsync(new SearchKnowledgeInput
        {
            KnowledgeBaseIds = definition.KnowledgeBaseIds,
            Query = userMessage,
            TopN = 4
        });
        var snippets = search.Data ?? [];
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

        return snippets.Count;
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
/// 单次运行的上下文（任务级项目归属、审批门参数等）
/// </summary>
public sealed record AgentRunContext(
    Guid? ProjectId = null,
    string? ExtraInstruction = null,
    // None/null=不启用审批（Chat 路径）；Interactive=SSE 可回传裁决；NonInteractive=按策略自动裁决
    string? ApprovalMode = null,
    Guid? TaskId = null,
    Guid? TaskLogId = null,
    string? UserId = null);
