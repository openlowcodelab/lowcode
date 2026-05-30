using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using H.Assistant.EntityFrameworkCore;

namespace H.Assistant.Data;

/// <summary>
/// Agent 和 Skill 数据初始化
/// </summary>
public class AgentSkillDataSeeder : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AgentDefinitionEntity, Guid> _agentRepository;
    private readonly IRepository<SkillDefinitionEntity, Guid> _skillRepository;

    public AgentSkillDataSeeder(
        IRepository<AgentDefinitionEntity, Guid> agentRepository,
        IRepository<SkillDefinitionEntity, Guid> skillRepository)
    {
        _agentRepository = agentRepository;
        _skillRepository = skillRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedSkillsAsync();
        await SeedAgentsAsync();
    }

    private async Task SeedSkillsAsync()
    {
        var query = await _skillRepository.GetQueryableAsync();
        var existingSkills = await query.ToListAsync();
        
        // 定义所有需要的 Skill
        var skillDefinitions = new List<SkillDefinitionEntity>
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
            }
        };

        // 检查并插入缺失的 Skill
        foreach (var skillDef in skillDefinitions)
        {
            if (!existingSkills.Any(s => s.SkillName == skillDef.SkillName))
            {
                await _skillRepository.InsertAsync(skillDef);
            }
        }
    }

    private async Task SeedAgentsAsync()
    {
        var query = await _agentRepository.GetQueryableAsync();
        var existingAgents = await query.ToListAsync();
        
        // 获取所有已插入的技能
        var skillQuery = await _skillRepository.GetQueryableAsync();
        var skills = await skillQuery.ToListAsync();
        var allSkillIds = skills.Select(s => s.Id).ToList();
        
        // 查找特定 Skill 的 ID（如果存在）
        //var browserSkillId = skills.FirstOrDefault(s => s.SkillName == "browser")?.Id;
        //var searchSkillId = skills.FirstOrDefault(s => s.SkillName == "search")?.Id;
        var dbSkillId = skills.FirstOrDefault(s => s.SkillName == "database")?.Id;
        var httpSkillId = skills.FirstOrDefault(s => s.SkillName == "http_client")?.Id;

        // 定义所有需要的 Agent
        var agentDefinitions = new List<AgentDefinitionEntity>
        {
            // 通用助手：使用所有 Skill
            new()
            {
                AgentType = "general",
                DisplayName = "通用助手",
                Description = "通用智能助手，支持问答、文本生成、代码编写等常见任务",
                SystemPrompt = "你是一个通用的智能助手，能够帮助用户解答问题、生成文本、编写代码、翻译语言等。请用简洁语言回答用户的问题，保持友好和专业的态度。",
                IsEnabled = true,
                SupportsStreaming = true,
                Temperature = 0.7f,
                MaxTokens = 2000,
                SkillIds = allSkillIds.ToJson()
            },
            // 客服助手：不使用数据库工具
            new()
            {
                AgentType = "customer-service",
                DisplayName = "客服助手",
                Description = "智能客服助手，支持问题解答、工单处理、客户反馈等客服场景",
                SystemPrompt = "你是一个专业的智能客服助手，负责解答用户问题、处理工单和收集客户反馈。请保持礼貌、耐心和专业的态度，用简洁清晰的语言回答用户。",
                IsEnabled = true,
                SupportsStreaming = true,
                Temperature = 0.5f,
                MaxTokens = 1500,
                SkillIds = allSkillIds.Where(id => id != dbSkillId).ToList().ToJson()
            },
            // 数据分析助手：只使用数据库工具和 HTTP 客户端
            new()
            {
                AgentType = "data-analysis",
                DisplayName = "数据分析助手",
                Description = "数据分析智能助手，支持数据查询、统计分析、报表生成等",
                SystemPrompt = "你是一个数据分析专家，能够帮助用户进行数据查询、统计分析、报表生成和趋势预测。请用专业的数据分析语言回答，并提供清晰的数据解释。",
                IsEnabled = true,
                SupportsStreaming = true,
                Temperature = 0.3f,
                MaxTokens = 3000,
                SkillIds = new[] { dbSkillId, httpSkillId }.Where(id => id.HasValue).Select(id => id!.Value).ToList().ToJson()
            },
            // 自动化测试助手：使用所有 Skill
            new()
            {
                AgentType = "automation-test",
                DisplayName = "自动化测试助手",
                Description = "自动化测试智能助手，支持测试用例生成、测试执行、结果分析等",
                SystemPrompt = "你是一个自动化测试专家，能够帮助用户生成测试用例、执行测试并分析结果。请提供专业的测试建议和详细的测试报告。",
                IsEnabled = true,
                SupportsStreaming = true,
                Temperature = 0.3f,
                MaxTokens = 3000,
                SkillIds = allSkillIds.ToJson()
            }
        };

        // 逐个检查并插入缺失的 Agent
        foreach (var agentDef in agentDefinitions)
        {
            if (!existingAgents.Any(a => a.AgentType == agentDef.AgentType))
            {
                await _agentRepository.InsertAsync(agentDef);
            }
        }
    }
}
