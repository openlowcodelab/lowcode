using H.Assistant.Application.Contracts;
using H.Testing.Application.Contracts;
using System.Text;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.Testing.Application;

/// <summary>
/// 测试项目知识库应用服务
/// 每个测试项目绑定一个 Assistant 知识库（懒创建），通过 Assistant 接口读写，描述项目功能与逻辑以辅助编写测试用例
/// </summary>
public class ProjectKnowledgeAppService : ApplicationService, IProjectKnowledgeAppService
{
    private const int MaxBaseNameLength = 100;
    private const int MaxNodeTitleLength = 200;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IKnowledgeBaseAppService _knowledgeBaseService;
    private readonly IKnowledgeDocumentAppService _knowledgeDocumentService;
    private readonly IProjectAppService _projectService;
    private readonly IAiCompletionAppService _aiCompletion;

    public ProjectKnowledgeAppService(
        IKnowledgeBaseAppService knowledgeBaseService,
        IKnowledgeDocumentAppService knowledgeDocumentService,
        IProjectAppService projectService,
        IAiCompletionAppService aiCompletion)
    {
        _knowledgeBaseService = knowledgeBaseService;
        _knowledgeDocumentService = knowledgeDocumentService;
        _projectService = projectService;
        _aiCompletion = aiCompletion;
    }

    public async Task<ProjectKnowledgeDto> GetOrCreateAsync(long projectId)
    {
        var knowledgeBase = await ResolveKnowledgeBaseAsync(projectId);
        return Map(knowledgeBase);
    }

    public async Task<List<ProjectKnowledgeNodeDto>> GetTreeAsync(long projectId)
    {
        var knowledgeBaseId = await ResolveKnowledgeBaseIdAsync(projectId);
        var tree = await _knowledgeDocumentService.GetTreeAsync(knowledgeBaseId);
        return tree.Select(MapNode).ToList();
    }

    public async Task<ProjectKnowledgeNodeDto> CreateNodeAsync(long projectId, CreateProjectKnowledgeNodeInput input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Title))
        {
            throw new UserFriendlyException("节点标题不能为空");
        }

        var knowledgeBaseId = await ResolveKnowledgeBaseIdAsync(projectId);
        var node = await _knowledgeDocumentService.CreateNodeAsync(new CreateKnowledgeNodeDto
        {
            KnowledgeBaseId = knowledgeBaseId,
            ParentId = input.ParentId,
            Title = Truncate(input.Title, MaxNodeTitleLength),
            NodeType = input.NodeType == "Directory" ? "Directory" : "Document"
        });
        return MapNode(node);
    }

    public async Task<ProjectKnowledgeNodeDto> RenameNodeAsync(Guid nodeId, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new UserFriendlyException("节点标题不能为空");
        }

        var node = await _knowledgeDocumentService.UpdateNodeAsync(nodeId, new UpdateKnowledgeNodeDto
        {
            Title = Truncate(title, MaxNodeTitleLength)
        });
        return MapNode(node);
    }

    public Task DeleteNodeAsync(Guid nodeId) => _knowledgeDocumentService.DeleteNodeAsync(nodeId);

    public async Task<string> GetDocumentContentAsync(Guid nodeId)
    {
        var document = await _knowledgeDocumentService.GetDocumentAsync(nodeId);
        return document?.Content ?? string.Empty;
    }

    public Task SaveDocumentAsync(Guid nodeId, string content) =>
        _knowledgeDocumentService.SaveDocumentAsync(nodeId, new SaveKnowledgeDocumentDto { Content = content ?? string.Empty });

    public async Task<AiKnowledgeDocumentDto> GenerateWithAiAsync(long projectId, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new UserFriendlyException("请输入需求描述");
        }

        var project = await _projectService.GetByIdAsync(projectId)
            ?? throw new UserFriendlyException("项目不存在");

        var result = await _aiCompletion.CompleteAsync(new AiCompletionInputDto
        {
            SystemPrompt = GenerateKnowledgeSystemPrompt,
            UserMessage = $"项目信息：\n名称：{project.Name}\n描述：{project.Description}\n\n用户的口语化描述：\n{description.Trim()}",
            Temperature = 0.3f,
            MaxTokens = 8192
        });

        var generated = ParseJson<AiKnowledgeDocumentDto>(result.Content);
        if (generated == null || (string.IsNullOrWhiteSpace(generated.Title) && string.IsNullOrWhiteSpace(generated.Content)))
        {
            throw new UserFriendlyException("AI 返回的知识内容无效，请补充描述后重试");
        }

        generated.Title = Truncate(string.IsNullOrWhiteSpace(generated.Title) ? "AI 生成知识" : generated.Title, MaxNodeTitleLength);
        generated.Content = generated.Content ?? string.Empty;
        return generated;
    }

    public async Task<string> GetKnowledgeDigestAsync(long projectId, int maxLength = 10000)
    {
        Guid knowledgeBaseId;
        try
        {
            knowledgeBaseId = await ResolveKnowledgeBaseIdAsync(projectId);
        }
        catch
        {
            return string.Empty;
        }

        List<KnowledgeNodeDto> tree;
        try
        {
            tree = await _knowledgeDocumentService.GetTreeAsync(knowledgeBaseId);
        }
        catch
        {
            return string.Empty;
        }

        var documents = new List<KnowledgeNodeDto>();
        CollectDocuments(tree, documents);
        if (documents.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var node in documents)
        {
            string? content;
            try
            {
                var document = await _knowledgeDocumentService.GetDocumentAsync(node.Id);
                content = document?.Content;
            }
            catch
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            var section = $"### {node.Title}\n{content.Trim()}\n";
            if (builder.Length + section.Length > maxLength)
            {
                var remaining = maxLength - builder.Length;
                if (remaining > 200)
                {
                    builder.Append(section[..remaining]).Append("…（内容过长已截断）");
                }

                break;
            }

            builder.Append(section);
        }

        return builder.ToString().Trim();
    }

    #region 私有方法

    /// <summary>
    /// 解析项目关联的知识库（返回完整信息）：Metadata 中不存在或对应库已被删除时自动创建并绑定。
    /// 创建后直接复用 CreateAsync 返回的 DTO，不再二次查询（新建记录在当前工作单元内可能尚未落库，立即查询会失败）
    /// </summary>
    private async Task<KnowledgeBaseDto> ResolveKnowledgeBaseAsync(long projectId)
    {
        var project = await _projectService.GetByIdAsync(projectId)
            ?? throw new UserFriendlyException("项目不存在");

        if (TryGetKnowledgeBaseId(project.KnowledgeBaseId, out var knowledgeBaseId))
        {
            try
            {
                return await _knowledgeBaseService.GetAsync(knowledgeBaseId);
            }
            catch
            {
                // 关联库已不存在，重建
            }
        }

        return await CreateAndBindKnowledgeBaseAsync(project);
    }

    /// <summary>
    /// 解析项目关联的知识库 ID（仅取 ID，不做存在性校验，供节点/文档等后续操作使用）
    /// </summary>
    private async Task<Guid> ResolveKnowledgeBaseIdAsync(long projectId)
    {
        var project = await _projectService.GetByIdAsync(projectId)
            ?? throw new UserFriendlyException("项目不存在");

        if (TryGetKnowledgeBaseId(project.KnowledgeBaseId, out var knowledgeBaseId))
        {
            return knowledgeBaseId;
        }

        var knowledgeBase = await CreateAndBindKnowledgeBaseAsync(project);
        return knowledgeBase.Id;
    }

    private async Task<KnowledgeBaseDto> CreateAndBindKnowledgeBaseAsync(ProjectDto project)
    {
        var knowledgeBase = await _knowledgeBaseService.CreateAsync(new CreateKnowledgeBaseDto
        {
            Name = Truncate($"测试知识库-{project.Name}-{project.Id}", MaxBaseNameLength),
            Description = Truncate($"测试项目「{project.Name}」的功能与逻辑知识，用于辅助编写测试用例", 500)
        });

        project.KnowledgeBaseId = knowledgeBase.Id.ToString();
        await _projectService.UpdateAsync(project.Id, project);
        return knowledgeBase;
    }

    private static bool TryGetKnowledgeBaseId(string? raw, out Guid knowledgeBaseId)
    {
        knowledgeBaseId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        return Guid.TryParse(raw.Trim().Trim('"'), out knowledgeBaseId);
    }

    private static void CollectDocuments(List<KnowledgeNodeDto> nodes, List<KnowledgeNodeDto> documents)
    {
        foreach (var node in nodes)
        {
            if (node.NodeType == "Document")
            {
                documents.Add(node);
            }

            if (node.Children is { Count: > 0 })
            {
                CollectDocuments(node.Children, documents);
            }
        }
    }

    private static ProjectKnowledgeDto Map(KnowledgeBaseDto knowledgeBase) => new()
    {
        KnowledgeBaseId = knowledgeBase.Id,
        Name = knowledgeBase.Name,
        DocumentCount = knowledgeBase.DocumentCount
    };

    private static ProjectKnowledgeNodeDto MapNode(KnowledgeNodeDto node) => new()
    {
        Id = node.Id,
        ParentId = node.ParentId,
        Title = node.Title,
        NodeType = node.NodeType,
        Children = node.Children.Select(MapNode).ToList()
    };

    /// <summary>
    /// 从 AI 返回文本中提取 JSON（去除 markdown 代码块标记与多余文本）
    /// </summary>
    private static string ExtractJson(string content)
    {
        var text = (content ?? string.Empty).Trim();

        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline > 0)
            {
                text = text[(firstNewline + 1)..];
            }

            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (lastFence > 0)
            {
                text = text[..lastFence];
            }
        }

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new UserFriendlyException("AI 返回的内容无法解析，请重试");
        }

        return text[start..(end + 1)];
    }

    private static T? ParseJson<T>(string content)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(ExtractJson(content), JsonOptions);
        }
        catch (JsonException)
        {
            throw new UserFriendlyException("AI 返回的内容无法解析，请重试");
        }
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        value = value.Trim();
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private const string GenerateKnowledgeSystemPrompt = """
        你是一名资深软件测试分析师。请根据用户的口语化描述，整理生成一份结构化的项目知识文档，用于描述被测项目的功能与逻辑，帮助测试人员编写测试用例。
        输出要求：
        1. 只输出一个 JSON 对象，不要输出任何解释文字，也不要使用 markdown 代码块标记。
        2. JSON 结构如下：
        {
          "title": "文档标题（简洁概括本知识主题）",
          "content": "markdown 格式的文档内容"
        }
        3. content 内容组织规则：
        - 按以下维度组织（缺失的维度可省略）：功能概述、核心业务流程、业务规则与约束、数据与状态说明、测试关注点。
        - 使用 markdown 标题、列表等结构化表达，避免大段口语化叙述。
        - 从用户描述中提取具体的流程步骤、校验规则、边界条件与异常场景；描述不清晰处做合理推断并保持保守。
        - 最后给出"测试关注点"小节，列出编写测试用例时应重点覆盖的场景。
        - 全部使用中文。
        """;

    #endregion
}
