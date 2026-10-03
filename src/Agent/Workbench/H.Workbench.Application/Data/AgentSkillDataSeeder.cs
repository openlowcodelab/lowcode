using H.Workbench.Core.Tools;
using H.Workbench.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace H.Workbench.Data;

/// <summary>
/// Agent 和 Skill 数据初始化。
/// 约定：内置技能在插入/同步前经 WorkbenchToolCatalog 校验实现类可加载，
/// 不可加载的技能降级为 Planned+禁用（LogError 提示），不再静默注册幽灵工具。
/// </summary>
public class AgentSkillDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AgentEntity, Guid> _agentRepository;
    private readonly IRepository<SkillEntity, Guid> _skillRepository;
    private readonly IRepository<ConnectorEntity, Guid> _connectorRepository;
    private readonly IRepository<AgentTemplateEntity, Guid> _templateRepository;
    private readonly IRepository<ApprovalRuleEntity, Guid> _approvalRuleRepository;
    private readonly IRepository<McpServerEntity, Guid> _mcpServerRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentSkillDataSeeder> _logger;

    public AgentSkillDataSeeder(
        IRepository<AgentEntity, Guid> agentRepository,
        IRepository<SkillEntity, Guid> skillRepository,
        IRepository<ConnectorEntity, Guid> connectorRepository,
        IRepository<AgentTemplateEntity, Guid> templateRepository,
        IRepository<ApprovalRuleEntity, Guid> approvalRuleRepository,
        IRepository<McpServerEntity, Guid> mcpServerRepository,
        IConfiguration configuration,
        ILogger<AgentSkillDataSeeder> logger)
    {
        _agentRepository = agentRepository;
        _skillRepository = skillRepository;
        _connectorRepository = connectorRepository;
        _templateRepository = templateRepository;
        _approvalRuleRepository = approvalRuleRepository;
        _mcpServerRepository = mcpServerRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var skills = await SeedSkillsAsync();
        await SeedConnectorsAsync();
        await SeedMcpServersAsync();
        await SeedTemplatesAsync(skills);
        await SeedApprovalRulesAsync();
    }

    /// <summary>
    /// 第一方 MCP 服务器登记。宿主已经把 H.Mcp.YunXiao 挂在 /yunxiao 上（工作项查询是真实实现），
    /// 但 McpServer 表原本一行种子都没有，员工侧永远连不到它——这里补上这条回路。
    /// 只按 Name 幂等插入：已存在就不覆盖，用户可能改过端点或凭据。
    /// </summary>
    private async Task SeedMcpServersAsync()
    {
        var query = await _mcpServerRepository.GetQueryableAsync();
        var existing = await query.ToListAsync();
        if (existing.Any(x => string.Equals(x.Name, "yunxiao", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        // 云效 PAT 由服务端配置（YunXiao 节）持有，MCP 连接本身不需要额外凭据。
        // 凭据缺失时仍登记但默认禁用：登记行是"有实现、等凭据"，
        // 让员工拿到一批必然报错的工具才是有害的。
        var hasCredential = !string.IsNullOrWhiteSpace(_configuration["YunXiao:PersonalAccessToken"])
                            && !string.IsNullOrWhiteSpace(_configuration["YunXiao:OrganizationId"]);

        var baseUrl = (_configuration["RemoteServices:Workbench:BaseUrl"] ?? "https://localhost:7065").TrimEnd('/');
        var endpoint = baseUrl + "/yunxiao";

        await _mcpServerRepository.InsertAsync(new McpServerEntity
        {
            Name = "yunxiao",
            DisplayName = "云效（平台内置）",
            Endpoint = endpoint,
            TransportType = "HTTP",
            TimeoutSeconds = 60,
            IsEnabled = hasCredential
        }, autoSave: true);

        if (hasCredential)
        {
            _logger.LogInformation("种子 MCP 服务器: yunxiao → {Endpoint}", endpoint);
        }
        else
        {
            _logger.LogWarning("已登记内置 MCP 服务器 yunxiao（{Endpoint}）但处于禁用状态：" +
                "appsettings 的 YunXiao:OrganizationId / YunXiao:PersonalAccessToken 未配置。" +
                "填好凭据后在 MCP 页启用即可使用工作项查询工具。", endpoint);
        }
    }

    /// <summary>
    /// 默认审批规则：技能级布尔只能"整个技能要不要批"，这几条把闸门收到真正会外溢影响的动作上
    /// （推远端、写库）。用户可在规则里改，已存在的同名规则不覆盖。
    /// </summary>
    private async Task SeedApprovalRulesAsync()
    {
        var defaults = new List<ApprovalRuleEntity>
        {
            new()
            {
                Name = "推送远端需人工批准",
                ToolPattern = "GitPushAsync",
                Effect = "Require",
                Priority = 20
            },
            new()
            {
                Name = "提交并推送需人工批准",
                ToolPattern = "GitCommitPushAsync",
                Effect = "Require",
                Priority = 20
            },
            new()
            {
                Name = "写数据库需人工批准",
                ToolPattern = "ExecuteCommandAsync",
                Effect = "Require",
                Priority = 20
            }
        };

        var existing = (await _approvalRuleRepository.GetListAsync()).Select(x => x.Name).ToHashSet();

        foreach (var rule in defaults.Where(r => !existing.Contains(r.Name)))
        {
            await _approvalRuleRepository.InsertAsync(rule, autoSave: true);
            _logger.LogInformation("种子审批规则: {Name} ({ToolPattern} → {Effect})", rule.Name, rule.ToolPattern, rule.Effect);
        }
    }

    /// <summary>
    /// 连接器登记。连接器现在是运行时相关的：绑定并启用后，WorkbenchToolCatalog.ConnectorSkillKeys
    /// 里映射的技能工具会授予该员工（只做加法，不会让存量员工失能）。
    /// 浏览器已具备真实执行体（Playwright），默认启用；计算机控制直接操作桌面，默认留关，由人决定。
    /// 已存在的行只刷新名称/描述/图标，开关状态归用户。
    /// </summary>
    private async Task SeedConnectorsAsync()
    {
        var query = await _connectorRepository.GetQueryableAsync();
        var existing = await query.ToListAsync();

        var definitions = new List<ConnectorEntity>
        {
            new()
            {
                ConnectorKey = "browser",
                ConnectorName = "浏览器",
                Description = "驱动真实浏览器（Playwright）：打开网页、点击、输入、读取渲染后文本、截图存盘；同时提供轻量 HTTP 抓取。",
                Source = "Market",
                Icon = "🌐",
                IsEnabled = true
            },
            new()
            {
                ConnectorKey = "computer",
                ConnectorName = "计算机控制",
                Description = "使用当前设备上的原生计算机自动化运行时：截屏、窗口列表与激活、鼠标点击、键盘输入。每个动作默认需人工批准。",
                Source = "Market",
                Icon = "🖥",
                IsEnabled = false
            }
        };

        foreach (var definition in definitions)
        {
            var matched = existing.FirstOrDefault(x => x.ConnectorKey == definition.ConnectorKey);
            if (matched is null)
            {
                await _connectorRepository.InsertAsync(definition);
                continue;
            }

            if (matched.Source != "Market") continue;

            matched.ConnectorName = definition.ConnectorName;
            matched.Description = definition.Description;
            matched.Icon = definition.Icon;

            // 浏览器连接器过去是占位（无执行体所以默认关）。现在它有了真实实现，
            // 而"关"从来不是用户点出来的——LastModificationTime 为空说明这行没被人动过，
            // 这种情况下按新默认值开启；被人手动改过的行一律尊重用户的选择。
            if (definition.IsEnabled && matched.LastModificationTime is null)
            {
                matched.IsEnabled = true;
            }

            await _connectorRepository.UpdateAsync(matched);
        }
    }

    private async Task SeedTemplatesAsync(List<SkillEntity> skills)
    {
        var query = await _templateRepository.GetQueryableAsync();
        var existingTemplates = await query.ToListAsync();

        List<Guid> SkillIds(params string[] names)
            => skills.Where(s => names.Contains(s.SkillName)).Select(s => s.Id).ToList();

        var templates = new List<AgentTemplateEntity>
        {
            new()
            {
                TemplateName = "研发工程师",
                Role = "研发工程师",
                Description = "负责前后端研发、构建自检与代码变更管理（流水线状态查询由云效 MCP 提供，只读）",
                SystemPrompt = "你是一名研发工程师，负责前后端代码开发与单元测试编写。仓库操作仅限系统提示词中列出的代码仓库。请按照工程规范完成任务：改动后用构建与测试自检确认可用，再在完成后给出变更摘要；没有跑过构建或测试就不要声称已完成。",
                SkillIds = JsonSerializer.Serialize(SkillIds("git", "workspace_file", "workspace_build", "test_runner")),
                IsBuiltin = true
            },
            new()
            {
                TemplateName = "测试工程师",
                Role = "测试工程师",
                Description = "负责测试用例设计、执行与缺陷跟踪",
                SystemPrompt = "你是一名测试工程师，负责测试用例设计、测试执行与缺陷跟踪。请覆盖正常、异常与边界场景，用测试自检工具实际执行而不是描述预期结果，并输出测试报告。",
                SkillIds = JsonSerializer.Serialize(SkillIds("browser", "workspace_file", "test_runner", "workspace_build")),
                IsBuiltin = true
            },
            new()
            {
                TemplateName = "数据分析师",
                Role = "数据分析师",
                Description = "负责数据查询、分析与报表输出",
                SystemPrompt = "你是一名数据分析师，负责数据查询、清洗与分析报告输出。请给出结论与可视化建议。",
                SkillIds = JsonSerializer.Serialize(SkillIds("database", "search")),
                IsBuiltin = true
            },
            new()
            {
                TemplateName = "办公助理",
                Role = "办公助理",
                Description = "负责周报/纪要/报表等文档产出，并把结论推送到群或邮箱",
                SystemPrompt = "你是一名办公助理，负责整理纪要、编写周报与产出报表。交付物要用文档与表格技能落成真正的 .docx/.xlsx 文件而不是聊天文本；需要通知他人时先用 NotifyListChannelsAsync 确认渠道，并如实说明发送对象与内容。",
                SkillIds = JsonSerializer.Serialize(SkillIds("office_document", "spreadsheet", "notify", "search")),
                IsBuiltin = true
            }
        };

        foreach (var template in templates)
        {
            var existing = existingTemplates.FirstOrDefault(x => x.TemplateName == template.TemplateName && x.IsBuiltin);
            if (existing is null)
            {
                await _templateRepository.InsertAsync(template);
            }
            else
            {
                // 内置模板随定义刷新（技能绑定、能力描述），用户另存的模板不动
                existing.SkillIds = template.SkillIds;
                existing.Description = template.Description;
                existing.SystemPrompt = template.SystemPrompt;
                existing.Role = template.Role;
                await _templateRepository.UpdateAsync(existing);
            }
        }
    }

    private async Task<List<SkillEntity>> SeedSkillsAsync()
    {
        var query = await _skillRepository.GetQueryableAsync();
        var existingSkills = await query.ToListAsync();

        // 定义所有需要的内置 Skill
        var skillDefinitions = new List<SkillEntity>
        {
            new()
            {
                SkillName = "browser",
                DisplayName = "浏览器工具",
                Description = "访问网页、提取文本和链接、检查 URL 可访问性",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.BrowserTool"
            },
            new()
            {
                SkillName = "search",
                DisplayName = "搜索工具",
                Description = "执行网络搜索，支持 Bing/Google/Baidu 搜索引擎和新闻搜索",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.SearchTool"
            },
            new()
            {
                SkillName = "database",
                DisplayName = "数据库工具",
                Description = "执行 SQL 查询、数据操作、获取表信息（SQL Server）。只能引用服务端登记的数据源名（Workbench:DataSources），不接受连接串；写操作默认需人工批准",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.DbTool"
            },
            new()
            {
                SkillName = "http_client",
                DisplayName = "HTTP 客户端",
                Description = "发送 HTTP GET/POST 请求，支持自定义请求头和查询参数",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.HttpClientTool"
            },
            new()
            {
                SkillName = "git",
                DisplayName = "Git 工具",
                Description = "在服务端工作目录内克隆仓库、切换分支、提交推送、查看提交记录",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.GitTool"
            },
            new()
            {
                SkillName = "workspace_file",
                DisplayName = "工作区文件工具",
                Description = "在 git 工作目录内的仓库副本中列出/读取/写入/搜索文件",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.WorkspaceFileTool"
            },
            new()
            {
                SkillName = "workspace_shell",
                DisplayName = "工作区命令行",
                Description = "在已克隆仓库目录内执行 shell 命令（构建/测试等）；目录白名单+超时+输出截断，默认需人工审批后执行",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.WorkspaceShellTool",
                RequiresApproval = true
            },
            new()
            {
                SkillName = "test_runner",
                DisplayName = "测试自检",
                Description = "在已克隆仓库内执行 dotnet test 并回传通过/失败计数（命令由服务端拼装，不接受自由命令）。默认免审批——改完能不能跑是要先回答的问题",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.TestRunnerTool",
                RequiresApproval = false
            },
            new()
            {
                SkillName = "workspace_build",
                DisplayName = "构建与脚手架",
                Description = "在已克隆仓库内执行 dotnet build/restore 并用 dotnet new 生成工程骨架；命令由服务端拼装、模板走白名单，默认免审批——编不过的代码不该等到人工审批才发现",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.WorkspaceBuildTool",
                RequiresApproval = false
            },
            new()
            {
                SkillName = "office_document",
                DisplayName = "Word 文档",
                Description = "在工作目录内生成/回读 .docx（标题、段落、列表、表格、引用、代码块）；生成后立即做结构校验，校验不过不留下文件。注意：pptx 尚未实现，需要演示文稿时先交 docx 大纲",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.OfficeDocumentTool",
                RequiresApproval = false
            },
            new()
            {
                SkillName = "spreadsheet",
                DisplayName = "表格处理",
                Description = "在工作目录内生成/回读 .xlsx 与 .csv（多工作表、表头、数字与文本自动区分）",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.SpreadsheetTool",
                RequiresApproval = false
            },
            new()
            {
                // 推送与发信都会外溢到组织里的真人，默认要人工批准；渠道 URL/密码留在服务端配置
                SkillName = "notify",
                DisplayName = "通知与邮件",
                Description = "向服务端登记的通知渠道推送消息（钉钉/飞书/企业微信机器人）或收发邮件（SMTP/IMAP）。模型只给渠道名，不给地址与凭据；外发动作默认需人工批准",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.NotifyTool",
                RequiresApproval = true
            },
            new()
            {
                // 直接操作本机桌面，没有沙箱可言：默认必须人工批准
                SkillName = "computer_control",
                DisplayName = "计算机控制",
                Description = "截屏、列出/激活窗口、移动与点击鼠标、键盘输入与组合键（Windows 桌面）。动作外溢到真实屏幕，默认需要人工批准",
                SkillType = "Function",
                ImplementationClass = "H.Workbench.Core.Tools.ComputerControlTool",
                RequiresApproval = true
            }
        };

        // 历史遗留的 pipeline 占位技能（Planned/"待实现"）在此删除：
        // 流水线能力改由云效 MCP 提供只读查询（列流水线、运行历史、单次运行详情），
        // 留着这行会让技能页永远挂一个"待实现"徽标，而它其实已经能用。
        var stalePipeline = existingSkills.FirstOrDefault(s => s.SkillName == "pipeline");
        if (stalePipeline is not null)
        {
            await _skillRepository.DeleteAsync(stalePipeline, autoSave: true);
            existingSkills.Remove(stalePipeline);
            _logger.LogInformation("已移除 pipeline 占位技能：流水线查询改由云效 MCP 提供");
        }

        foreach (var def in skillDefinitions)
        {
            // 实现类校验：不可加载则降级为 Planned+禁用（不抛异常，避免炸宿主启动）
            if (!string.IsNullOrWhiteSpace(def.ImplementationClass))
            {
                if (!WorkbenchToolCatalog.TryValidate(def.ImplementationClass, out _, out var error))
                {
                    def.IsEnabled = false;
                    def.SkillType = "Planned";
                    def.Config = JsonSerializer.Serialize(new { unimplemented = true, reason = error });
                    _logger.LogError("技能 {SkillName} 的实现类 {ClassName} 不可加载：{Error}，已标记为未实现",
                        def.SkillName, def.ImplementationClass, error);
                }
                else
                {
                    def.IsEnabled = true;
                }
            }
            else
            {
                def.IsEnabled = false;
            }

            // RequiresApproval 由各技能定义自带（workspace_shell=true）；
            // 更新分支不回写该字段，保留用户在技能页的手动选择

            var existing = existingSkills.FirstOrDefault(s => s.SkillName == def.SkillName);
            if (existing is null)
            {
                await _skillRepository.InsertAsync(def);
                existingSkills.Add(def);
            }
            else
            {
                // 内置技能行随定义刷新（存量库的 pipeline=true / git 幽灵行在此被纠正）
                existing.IsEnabled = def.IsEnabled;
                existing.SkillType = def.SkillType;
                existing.Description = def.Description;
                existing.ImplementationClass = def.ImplementationClass;
                existing.Config = def.Config;
                await _skillRepository.UpdateAsync(existing);
            }
        }

        return existingSkills;
    }
}
