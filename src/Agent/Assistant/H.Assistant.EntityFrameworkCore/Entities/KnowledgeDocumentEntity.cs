using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// 知识库文档内容实体（与 KnowledgeNodeEntity 1:1 关联）
/// </summary>
public class KnowledgeDocumentEntity : CreationAuditedEntity<Guid>
{
    /// <summary> 关联的节点 ID </summary>
    public Guid? NodeId { get; set; }

    /// <summary> Markdown 内容 </summary>
    public string? Content { get; set; }
}
