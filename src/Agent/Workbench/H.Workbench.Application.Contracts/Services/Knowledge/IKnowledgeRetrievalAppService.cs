using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 知识库检索服务：为员工运行时提供关键词召回（无向量库，LIKE 召回 + 词频打分）
/// </summary>
public interface IKnowledgeRetrievalAppService : IAppService
{
    Task<BaseOutput<List<KnowledgeSnippetDto>>> SearchAsync(SearchKnowledgeInput input);
}

public class SearchKnowledgeInput
{
    public List<Guid> KnowledgeBaseIds { get; set; } = new();
    public string Query { get; set; } = string.Empty;
    public int TopN { get; set; } = 4;
    public int SnippetMaxChars { get; set; } = 900;
}

public class KnowledgeSnippetDto
{
    public Guid KnowledgeBaseId { get; set; }
    public string KnowledgeBaseName { get; set; } = string.Empty;
    public Guid NodeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Snippet { get; set; } = string.Empty;
    public double Score { get; set; }
}
