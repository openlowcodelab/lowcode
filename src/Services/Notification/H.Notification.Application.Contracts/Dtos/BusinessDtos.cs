using H.Abp.Application.Contracts;

namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知模板DTO
/// </summary>
public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public NotificationChannelType ChannelType { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 通知业务DTO
/// </summary>
public class NotificationBusinessDto : AuditedEntityDto<Guid>
{
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// 完整业务编码（分类Id-编码后缀）
    /// </summary>
    public string BusinessCode { get; set; } = string.Empty;

    public string? Description { get; set; }
    public NotificationLevel DefaultLevel { get; set; }
    public bool IsEnabled { get; set; }

    public List<NotificationTemplateDto> Templates { get; set; } = new();

    /// <summary>
    /// 已配置渠道（跨级别去重，用于列表展示）
    /// </summary>
    public List<NotificationChannelType> ConfiguredChannels { get; set; } = new();

    /// <summary>
    /// 绑定的联系人组数量
    /// </summary>
    public int GroupCount { get; set; }
}

/// <summary>
/// 创建通知业务DTO
/// </summary>
public class CreateNotificationBusinessDto
{
    /// <summary>
    /// 所属分类ID
    /// </summary>
    public long CategoryId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// 业务编码后缀（3-16位小写字母），保存时自动拼接为 "分类Id-后缀"
    /// </summary>
    public string CodeSuffix { get; set; } = string.Empty;

    public string? Description { get; set; }
    public NotificationLevel DefaultLevel { get; set; } = NotificationLevel.Normal;
    public bool IsEnabled { get; set; } = true;

    public List<NotificationTemplateDto> Templates { get; set; } = new();
}

/// <summary>
/// 更新通知业务DTO
/// </summary>
public class UpdateNotificationBusinessDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public NotificationLevel DefaultLevel { get; set; }
    public bool IsEnabled { get; set; }
    public List<NotificationTemplateDto> Templates { get; set; } = new();
}

/// <summary>
/// 通知业务查询参数
/// </summary>
public class NotificationBusinessQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }

    /// <summary>
    /// 按分类过滤（为空则查全部）
    /// </summary>
    public long? CategoryId { get; set; }
}
