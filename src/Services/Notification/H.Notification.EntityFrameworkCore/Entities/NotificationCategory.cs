using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Notification.EntityFrameworkCore;

/// <summary>
/// 通知分类实体（主键 long，从 1000 自增）
/// </summary>
public class NotificationCategory : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 分类名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序值（升序）
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}
