using H.Notification.Application.Contracts;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Notification.EntityFrameworkCore;

/// <summary>
/// 通知渠道实体（可复用的 provider 配置）
/// </summary>
public class NotificationChannelEntity : FullAuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 渠道类型
    /// </summary>
    public NotificationChannelType ChannelType { get; set; }

    /// <summary>
    /// 渠道名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 渠道编码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 渠道配置（JSON，各渠道 provider 参数）
    /// </summary>
    public string? ConfigJson { get; set; }
}
