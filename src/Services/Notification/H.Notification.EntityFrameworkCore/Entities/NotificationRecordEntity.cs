using H.Notification.Application.Contracts;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Notification.EntityFrameworkCore;

/// <summary>
/// 通知记录主记录实体（一次通知事件）
/// </summary>
public class NotificationRecordEntity : CreationAuditedEntity<Guid>, IMultiTenant
{
    public NotificationRecordEntity()
    {
    }

    public NotificationRecordEntity(Guid id) : base(id)
    {
    }

    public virtual Guid? TenantId { get; set; }

    public Guid BusinessId { get; set; }
    public string? BusinessName { get; set; }
    public string? BusinessCode { get; set; }
    public NotificationLevel Level { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? DataJson { get; set; }
    public string? TriggerSource { get; set; }
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }

    public virtual ICollection<InAppRecordEntity> InAppRecords { get; set; } = new List<InAppRecordEntity>();
    public virtual ICollection<EmailRecordEntity> EmailRecords { get; set; } = new List<EmailRecordEntity>();
    public virtual ICollection<SmsRecordEntity> SmsRecords { get; set; } = new List<SmsRecordEntity>();
    public virtual ICollection<WebhookRecordEntity> WebhookRecords { get; set; } = new List<WebhookRecordEntity>();
}

/// <summary>
/// 站内信记录实体
/// </summary>
public class InAppRecordEntity : Entity<Guid>, IMultiTenant
{
    public InAppRecordEntity() { }
    public InAppRecordEntity(Guid id) : base(id) { }

    public virtual Guid? TenantId { get; set; }
    public Guid RecordId { get; set; }
    public NotificationLevel Level { get; set; }
    public string? BusinessName { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? SentTime { get; set; }

    /// <summary>
    /// 站内信目标用户标识
    /// </summary>
    public string? TargetUserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadTime { get; set; }

    public virtual NotificationRecordEntity? Record { get; set; }
}

/// <summary>
/// 邮件记录实体
/// </summary>
public class EmailRecordEntity : Entity<Guid>, IMultiTenant
{
    public EmailRecordEntity() { }
    public EmailRecordEntity(Guid id) : base(id) { }

    public virtual Guid? TenantId { get; set; }
    public Guid RecordId { get; set; }
    public NotificationLevel Level { get; set; }
    public string? BusinessName { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? SentTime { get; set; }

    public string? ToAddress { get; set; }

    public virtual NotificationRecordEntity? Record { get; set; }
}

/// <summary>
/// 短信记录实体
/// </summary>
public class SmsRecordEntity : Entity<Guid>, IMultiTenant
{
    public SmsRecordEntity() { }
    public SmsRecordEntity(Guid id) : base(id) { }

    public virtual Guid? TenantId { get; set; }
    public Guid RecordId { get; set; }
    public NotificationLevel Level { get; set; }
    public string? BusinessName { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? SentTime { get; set; }

    public string? Phone { get; set; }

    public virtual NotificationRecordEntity? Record { get; set; }
}

/// <summary>
/// Webhook记录实体
/// </summary>
public class WebhookRecordEntity : Entity<Guid>, IMultiTenant
{
    public WebhookRecordEntity() { }
    public WebhookRecordEntity(Guid id) : base(id) { }

    public virtual Guid? TenantId { get; set; }
    public Guid RecordId { get; set; }
    public NotificationLevel Level { get; set; }
    public string? BusinessName { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? SentTime { get; set; }

    public string? Url { get; set; }
    public int? HttpStatus { get; set; }

    public virtual NotificationRecordEntity? Record { get; set; }
}
