using System.Text.Json;
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
        if (await query.AnyAsync())
        {
            return; // 已有数据，跳过
        }

        var weatherSkill = new SkillDefinitionEntity
        {
            SkillName = "weather_query",
            DisplayName = "天气查询",
            Description = "查询指定城市的天气信息",
            SkillType = "Function",
            ImplementationClass = "H.Assistant.Core.Tools.WeatherTool",
            IsEnabled = true,
            RequiresApproval = false
        };

        var calcSkill = new SkillDefinitionEntity
        {
            SkillName = "calculator",
            DisplayName = "计算器",
            Description = "执行数学计算，支持加减乘除",
            SkillType = "Function",
            ImplementationClass = "H.Assistant.Core.Tools.CalculatorTool",
            IsEnabled = true,
            RequiresApproval = false
        };

        var dateTimeSkill = new SkillDefinitionEntity
        {
            SkillName = "datetime_query",
            DisplayName = "时间查询",
            Description = "获取当前日期时间或计算日期差",
            SkillType = "Function",
            ImplementationClass = "H.Assistant.Core.Tools.DateTimeTool",
            IsEnabled = true,
            RequiresApproval = false
        };

        await _skillRepository.InsertAsync(weatherSkill);
        await _skillRepository.InsertAsync(calcSkill);
        await _skillRepository.InsertAsync(dateTimeSkill);
    }

    private async Task SeedAgentsAsync()
    {
        var query = await _agentRepository.GetQueryableAsync();
        if (await query.AnyAsync())
        {
            return; // 已有数据，跳过
        }

        // 获取已插入的技能
        var skillQuery = await _skillRepository.GetQueryableAsync();
        var skills = await skillQuery.ToListAsync();
        var allSkillIds = skills.Select(s => s.Id).ToList();
        var noCalcSkillIds = allSkillIds.Where(id => id != skills.First(s => s.SkillName == "calculator").Id).ToList();
        var calcSkillOnly = new List<Guid> { skills.First(s => s.SkillName == "calculator").Id };

        var generalAgent = new AgentDefinitionEntity
        {
            AgentType = "general",
            DisplayName = "通用助手",
            Description = "通用智能助手，支持问答、文本生成、代码编写等常见任务",
            SystemPrompt = "你是一个通用的智能助手，能够帮助用户解答问题、生成文本、编写代码、翻译语言等。请用简洁语言回答用户的问题，保持友好和专业的态度。",
            IsEnabled = true,
            SupportsStreaming = true,
            Temperature = 0.7f,
            MaxTokens = 2000,
            SkillIds = JsonSerializer.Serialize(allSkillIds)
        };

        var customerServiceAgent = new AgentDefinitionEntity
        {
            AgentType = "customer-service",
            DisplayName = "客服助手",
            Description = "智能客服助手，支持问题解答、工单处理、客户反馈等客服场景",
            SystemPrompt = "你是一个专业的智能客服助手，负责解答用户问题、处理工单和收集客户反馈。请保持礼貌、耐心和专业的态度，用简洁清晰的语言回答用户。",
            IsEnabled = true,
            SupportsStreaming = true,
            Temperature = 0.5f,
            MaxTokens = 1500,
            SkillIds = JsonSerializer.Serialize(noCalcSkillIds)
        };

        var dataAnalysisAgent = new AgentDefinitionEntity
        {
            AgentType = "data-analysis",
            DisplayName = "数据分析助手",
            Description = "数据分析智能助手，支持数据查询、统计分析、报表生成等",
            SystemPrompt = "你是一个数据分析专家，能够帮助用户进行数据查询、统计分析、报表生成和趋势预测。请用专业的数据分析语言回答，并提供清晰的数据解释。",
            IsEnabled = true,
            SupportsStreaming = true,
            Temperature = 0.3f,
            MaxTokens = 3000,
            SkillIds = JsonSerializer.Serialize(calcSkillOnly)
        };

        await _agentRepository.InsertAsync(generalAgent);
        await _agentRepository.InsertAsync(customerServiceAgent);
        await _agentRepository.InsertAsync(dataAnalysisAgent);
    }
}
