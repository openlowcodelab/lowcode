using Volo.Abp.Application.Dtos;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 知识库节点 DTO（树结构，不含文档内容）
/// </summary>
public class KnowledgeNodeDto : CreationAuditedEntityDto<Guid>
{
    public Guid? ParentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string NodeType { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<KnowledgeNodeDto> Children { get; set; } = new();
}
