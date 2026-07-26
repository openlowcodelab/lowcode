using H.Abp.Application.Contracts;

namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知分类DTO
/// </summary>
public class NotificationCategoryDto : FullAuditedEntityDto<long>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Sort { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 该分类下的业务数量
    /// </summary>
    public int BusinessCount { get; set; }
}

/// <summary>
/// 创建通知分类DTO
/// </summary>
public class CreateNotificationCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Sort { get; set; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新通知分类DTO
/// </summary>
public class UpdateNotificationCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Sort { get; set; }
    public bool IsEnabled { get; set; }
}
