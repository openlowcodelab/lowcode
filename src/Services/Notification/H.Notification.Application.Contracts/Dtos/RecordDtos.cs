using H.Abstractions;

namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知记录主记录DTO
/// </summary>
public class NotificationRecordDto : EntityDto<Guid>
{
    public Guid BusinessId { get; set; }
    public string? BusinessName { get; set; }
    public string? BusinessCode { get; set; }
    public NotificationLevel Level { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? TriggerSource { get; set; }
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 渠道记录基类DTO
/// </summary>
public abstract class ChannelRecordDtoBase : EntityDto<Guid>
{
    public Guid RecordId { get; set; }
    public string? BusinessName { get; set; }
    public NotificationLevel Level { get; set; }
    public Guid? ContactId { get; set; }
    public string? ContactName { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DeliveryStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? SentTime { get; set; }
}

/// <summary>
/// 站内信记录DTO
/// </summary>
public class InAppRecordDto : ChannelRecordDtoBase
{
    public string? TargetUserId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadTime { get; set; }
}

/// <summary>
/// 邮件记录DTO
/// </summary>
public class EmailRecordDto : ChannelRecordDtoBase
{
    public string? ToAddress { get; set; }
}

/// <summary>
/// 短信记录DTO
/// </summary>
public class SmsRecordDto : ChannelRecordDtoBase
{
    public string? Phone { get; set; }
}

/// <summary>
/// Webhook记录DTO
/// </summary>
public class WebhookRecordDto : ChannelRecordDtoBase
{
    public string? Url { get; set; }
    public int? HttpStatus { get; set; }
}

/// <summary>
/// 主记录查询参数
/// </summary>
public class NotificationRecordQueryDto : PagedResultRequestDto
{
    public Guid? BusinessId { get; set; }
    public NotificationLevel? Level { get; set; }
}

/// <summary>
/// 渠道记录查询参数
/// </summary>
public class ChannelRecordQueryDto : PagedResultRequestDto
{
    public DeliveryStatus? Status { get; set; }
    public string? Filter { get; set; }
}
