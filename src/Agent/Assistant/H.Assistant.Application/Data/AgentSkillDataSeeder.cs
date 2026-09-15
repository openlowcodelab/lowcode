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

    public AgentSkillDataSeeder(
        IRepository<AgentEntity, Guid> agentRepository,
        IRepository<SkillEntity, Guid> skillRepository)
    {
        _agentRepository = agentRepository;
        _skillRepository = skillRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedSkillsAsync();
    }

    private async Task SeedSkillsAsync()
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
            }
        }
    }
}
