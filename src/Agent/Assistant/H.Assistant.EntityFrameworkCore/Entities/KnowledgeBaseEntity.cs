using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// 知识库实体（知识文档的顶层容器）
/// </summary>
public class KnowledgeBaseEntity : CreationAuditedEntity<Guid>
{
    /// <summary> 名称 </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary> 描述 </summary>
    public string? Description { get; set; }

    /// <summary> 排序序号 </summary>
    public int SortOrder { get; set; }
}
