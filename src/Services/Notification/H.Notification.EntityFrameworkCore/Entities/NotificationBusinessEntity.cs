using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Notification.EntityFrameworkCore;

/// <summary>
/// 通知业务实体
/// </summary>
public class NotificationBusinessEntity : FullAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户ID（多租户）
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 业务名称
    /// </summary>
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// 业务编码
    /// </summary>
    public string BusinessCode { get; set; } = string.Empty;

    /// <summary>
    /// 业务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 通知方式配置集合
    /// </summary>
    public virtual ICollection<NotificationMethodConfigEntity> Methods { get; set; } = new List<NotificationMethodConfigEntity>();
}

/// <summary>
/// 通知方式配置实体
/// </summary>
public class NotificationMethodConfigEntity : FullAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户ID（多租户）
    /// </summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>
    /// 关联的业务ID
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <summary>
    /// 通知方式类型
    /// </summary>
    public int MethodType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 配置值（JSON格式）
    /// </summary>
    public string? ConfigValue { get; set; }

    /// <summary>
    /// Webhook地址
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// 短信模板ID
    /// </summary>
    public string? SmsTemplateId { get; set; }

    /// <summary>
    /// 邮件地址
    /// </summary>
    public string? EmailAddress { get; set; }

    /// <summary>
    /// 关联的通知业务
    /// </summary>
    public virtual NotificationBusinessEntity? Business { get; set; }
}
