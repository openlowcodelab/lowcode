using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 知识库 DTO
/// </summary>
public class KnowledgeBaseDto : CreationAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }

    /// <summary> 包含的文档数量（Document 节点数） </summary>
    public int DocumentCount { get; set; }
}
