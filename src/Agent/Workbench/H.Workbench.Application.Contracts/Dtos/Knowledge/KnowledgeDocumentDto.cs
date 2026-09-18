using H.Abp.Application.Contracts;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 知识库文档内容 DTO
/// </summary>
public class KnowledgeDocumentDto : CreationAuditedEntityDto<Guid>
{
    public Guid NodeId { get; set; }
    public string? Content { get; set; }
}
