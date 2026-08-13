namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试项目知识库信息（与 Assistant 知识库一对一绑定）
/// </summary>
public class ProjectKnowledgeDto
{
    /// <summary> 关联的 Assistant 知识库 ID </summary>
    public Guid KnowledgeBaseId { get; set; }

    /// <summary> 知识库名称 </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary> 文档数量 </summary>
    public int DocumentCount { get; set; }
}

/// <summary>
/// 知识库节点（树结构，不含文档内容）
/// </summary>
public class ProjectKnowledgeNodeDto
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary> Directory / Document </summary>
    public string NodeType { get; set; } = "Directory";

    public List<ProjectKnowledgeNodeDto> Children { get; set; } = [];
}

/// <summary>
/// AI 生成的知识文档（预览后保存）
/// </summary>
public class AiKnowledgeDocumentDto
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
