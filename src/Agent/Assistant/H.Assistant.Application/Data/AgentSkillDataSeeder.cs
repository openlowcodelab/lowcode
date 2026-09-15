using H.Assistant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace H.Assistant.Data;

/// <summary>
/// Agent 和 Skill 数据初始化
/// </summary>
public class AgentSkillDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AgentEntity, Guid> _agentRepository;
    private readonly IRepository<SkillEntity, Guid> _skillRepository;
    private readonly IRepository<ConnectorEntity, Guid> _connectorRepository;
    private readonly IRepository<AgentTemplateEntity, Guid> _templateRepository;

    public AgentSkillDataSeeder(
        IRepository<AgentEntity, Guid> agentRepository,
        IRepository<SkillEntity, Guid> skillRepository,
        IRepository<ConnectorEntity, Guid> connectorRepository,
        IRepository<AgentTemplateEntity, Guid> templateRepository)
    {
        _agentRepository = agentRepository;
        _skillRepository = skillRepository;
        _connectorRepository = connectorRepository;
        _templateRepository = templateRepository;
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
                Description = "负责前后端研发、测试、CI/CD 流水线部署",
                SystemPrompt = "你是一名研发工程师，负责前后端代码开发、单元测试编写与 CI/CD 流水线部署。请按照工程规范完成任务，并在完成后给出变更摘要。",
                SkillIds = System.Text.Json.JsonSerializer.Serialize(SkillIds("git", "workspace_file", "pipeline")),
                IsBuiltin = true
            },
            new()
            {
                TemplateName = "测试工程师",
                Role = "测试工程师",
                Description = "负责测试用例设计、执行与缺陷跟踪",
                SystemPrompt = "你是一名测试工程师，负责测试用例设计、测试执行与缺陷跟踪。请覆盖正常、异常与边界场景，并输出测试报告。",
                SkillIds = System.Text.Json.JsonSerializer.Serialize(SkillIds("browser", "workspace_file")),
                IsBuiltin = true
            },
            new()
            {
                TemplateName = "数据分析师",
                Role = "数据分析师",
                Description = "负责数据查询、分析与报表输出",
                SystemPrompt = "你是一名数据分析师，负责数据查询、清洗与分析报告输出。请给出结论与可视化建议。",
                SkillIds = System.Text.Json.JsonSerializer.Serialize(SkillIds("database", "search")),
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
            else if (string.IsNullOrWhiteSpace(existing.SkillIds) || existing.SkillIds == "[]")
            {
                existing.SkillIds = template.SkillIds;
                await _templateRepository.UpdateAsync(existing);
            }
        }
    }

    private async Task<List<SkillEntity>> SeedSkillsAsync()
    {
        var query = await _skillRepository.GetQueryableAsync();
        var existingSkills = await query.ToListAsync();

        // 定义所有需要的 Skill
        var skillDefinitions = new List<SkillEntity>
        {
            new()
            {
                SkillName = "browser",
                DisplayName = "浏览器工具",
                Description = "访问网页、提取文本和链接、检查 URL 可访问性",
                SkillType = "Function",
                ImplementationClass = "H.Assistant.Core.Tools.BrowserTool",
                IsEnabled = true,
                RequiresApproval = false
            },
            new()
            {
                SkillName = "search",
                DisplayName = "搜索工具",
                Description = "执行网络搜索，支持 Bing/Google/Baidu 搜索引擎和新闻搜索",
                SkillType = "Function",
                ImplementationClass = "H.Assistant.Core.Tools.SearchTool",
                IsEnabled = true,
                RequiresApproval = false
            },
            new()
            {
                SkillName = "database",
                DisplayName = "数据库工具",
                Description = "执行 SQL 查询、数据操作、获取表信息，支持 SQL Server",
                SkillType = "Function",
                ImplementationClass = "H.Assistant.Core.Tools.DbTool",
                IsEnabled = true,
                RequiresApproval = false
            },
            new()
            {
                SkillName = "http_client",
                DisplayName = "HTTP 客户端",
                Description = "发送 HTTP GET/POST 请求，支持自定义请求头和查询参数",
                SkillType = "Function",
                ImplementationClass = "H.Assistant.Core.Tools.HttpClientTool",
                IsEnabled = true,
                RequiresApproval = false
            },
            new()
            {
                SkillName = "git",
                DisplayName = "Git 工具",
                Description = "克隆仓库、创建分支、提交推送、合并分支、查看提交记录（需要助手运行时的资源上下文）",
                SkillType = "Function",
                ImplementationClass = "H.Assistant.Core.Tools.GitTool",
                IsEnabled = true,
                RequiresApproval = false
            },
            new()
            {
                SkillName = "workspace_file",
                DisplayName = "工作区文件工具",
                Description = "在 git 资源的本地克隆内列出/读取/写入文件",
                SkillType = "Function",
                ImplementationClass = "H.Assistant.Core.Tools.WorkspaceFileTool",
                IsEnabled = true,
                RequiresApproval = false
            },
            new()
            {
                SkillName = "pipeline",
                DisplayName = "流水线工具",
                Description = "触发 DevOps 流水线、查询流水线状态、调用资源 HTTP API（需要助手运行时的资源上下文）",
                SkillType = "Function",
                ImplementationClass = "H.Assistant.Core.Tools.PipelineTool",
                IsEnabled = true,
                RequiresApproval = false
            }
        };

        // 检查并插入缺失的 Skill
        foreach (var skillDef in skillDefinitions)
        {
            if (!existingSkills.Any(s => s.SkillName == skillDef.SkillName))
            {
                await _skillRepository.InsertAsync(skillDef);
                existingSkills.Add(skillDef);
            }
        }

        return existingSkills;
    }
}
