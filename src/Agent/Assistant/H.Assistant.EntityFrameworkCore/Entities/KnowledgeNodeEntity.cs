using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// 知识库节点实体（目录树结构）
/// </summary>
public class KnowledgeNodeEntity : CreationAuditedEntity<Guid>
{
    /// <summary> 父节点 ID，null 表示根节点 </summary>
    public Guid? ParentId { get; set; }

    /// <summary> 标题 </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary> 节点类型：Directory / Document </summary>
    public string NodeType { get; set; } = "Directory";

    /// <summary> 归属类型：Knowledge / Memory </summary>
    public string OwnerType { get; set; } = "Knowledge";

    /// <summary> 排序序号 </summary>
    public int SortOrder { get; set; }
}
