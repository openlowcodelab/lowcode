using H.Abstractions;

namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知渠道DTO
/// </summary>
public class NotificationChannelDto : FullAuditedEntityDto<Guid>
{
    public NotificationChannelType ChannelType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 渠道配置（JSON，各渠道 provider 参数）
    /// </summary>
    public string? ConfigJson { get; set; }
}

/// <summary>
/// 创建通知渠道DTO
/// </summary>
public class CreateNotificationChannelDto
{
    public NotificationChannelType ChannelType { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? ConfigJson { get; set; }
}

/// <summary>
/// 更新通知渠道DTO
/// </summary>
public class UpdateNotificationChannelDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public string? ConfigJson { get; set; }
}

/// <summary>
/// 通知渠道查询参数
/// </summary>
public class NotificationChannelQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}
