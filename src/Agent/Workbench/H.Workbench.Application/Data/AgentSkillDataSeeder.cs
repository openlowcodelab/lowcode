using H.Workbench.Core.Tools;
using H.Workbench.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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
    private readonly ILogger<AgentSkillDataSeeder> _logger;

    public AgentSkillDataSeeder(
        IRepository<AgentEntity, Guid> agentRepository,
        IRepository<SkillEntity, Guid> skillRepository,
        IRepository<ConnectorEntity, Guid> connectorRepository,
        IRepository<AgentTemplateEntity, Guid> templateRepository,
        ILogger<AgentSkillDataSeeder> logger)
    {
        _agentRepository = agentRepository;
        _skillRepository = skillRepository;
        _connectorRepository = connectorRepository;
        _templateRepository = templateRepository;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var skills = await SeedSkillsAsync();
        await SeedConnectorsAsync();
        await SeedTemplatesAsync(skills);
    }

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
                Description = "连接浏览器，让员工可以控制浏览器。",
                Source = "Market",
                Icon = "🌐",
                IsEnabled = false
            },
            new()
            {
                ConnectorKey = "computer",
                ConnectorName = "计算机控制",
                Description = "使用当前设备上的原生计算机自动化运行时。",
                Source = "Market",
                Icon = "🖥",
                IsEnabled = false
            }
        };

        foreach (var definition in definitions)
        {
            if (!existing.Any(x => x.ConnectorKey == definition.ConnectorKey))
            {
                await _connectorRepository.InsertAsync(definition);
            }
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
                Description = "负责前后端研发与代码变更管理（流水线能力规划中）",
                SystemPrompt = "你是一名研发工程师，负责前后端代码开发与单元测试编写。仓库操作仅限系统提示词中列出的代码仓库。请按照工程规范完成任务，并在完成后给出变更摘要。",
                SkillIds = JsonSerializer.Serialize(SkillIds("git", "workspace_file")),
                IsBuiltin = true
            },
            new()
            {
                TemplateName = "测试工程师",
                Role = "测试工程师",
                Description = "负责测试用例设计、执行与缺陷跟踪",
                SystemPrompt = "你是一名测试工程师，负责测试用例设计、测试执行与缺陷跟踪。请覆盖正常、异常与边界场景，并输出测试报告。",
                SkillIds = JsonSerializer.Serialize(SkillIds("browser", "workspace_file")),
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
                Description = "执行 SQL 查询、数据操作、获取表信息，支持 SQL Server",
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
                // 云效等 DevOps 流水线 API 尚未接入，明确标记为待实现，避免幽灵注册
                SkillName = "pipeline",
                DisplayName = "流水线工具",
                Description = "（待接入）触发 DevOps 流水线、查询流水线状态",
                SkillType = "Planned",
                ImplementationClass = null
            }
        };

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

            def.RequiresApproval = false;

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
