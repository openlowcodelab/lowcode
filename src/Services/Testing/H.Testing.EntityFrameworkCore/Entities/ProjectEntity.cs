using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 测试项目
/// </summary>
public class ProjectEntity : AuditedEntity<long>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>项目名称</summary>
    public string Name { get; set; } = string.Empty;

    public string? KnowledgeBaseId { get; set; }

    /// <summary>项目状态</summary>
    public int Status { get; set; }

    /// <summary>项目描述</summary>
    public string? Description { get; set; }
}
