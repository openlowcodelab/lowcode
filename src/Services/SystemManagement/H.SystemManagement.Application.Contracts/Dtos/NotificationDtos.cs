using H.SystemManagement.Application.Contracts.Enums;
using Volo.Abp.Application.Dtos;

namespace H.SystemManagement.Application.Contracts.Dtos;

/// <summary>
/// 通知业务配置DTO
/// </summary>
public class NotificationBusinessDto : FullAuditedEntityDto<Guid>
{
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
    /// 通知方式配置列表
    /// </summary>
    public List<NotificationMethodConfigDto> Methods { get; set; } = new();
}

/// <summary>
/// 创建通知业务DTO
/// </summary>
public class CreateNotificationBusinessDto
{
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
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新通知业务DTO
/// </summary>
public class UpdateNotificationBusinessDto
{
    /// <summary>
    /// 业务名称
    /// </summary>
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// 业务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 通知方式配置DTO
/// </summary>
public class NotificationMethodConfigDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 关联的业务ID
    /// </summary>
    public Guid BusinessId { get; set; }

    /// <summary>
    /// 通知方式类型
    /// </summary>
    public NotificationMethodType MethodType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 配置值（JSON格式）
    /// </summary>
    public string? ConfigValue { get; set; }

    /// <summary>
    /// Webhook地址（当MethodType=Webhook时使用）
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// 短信模板ID（当MethodType=Sms时使用）
    /// </summary>
    public string? SmsTemplateId { get; set; }

    /// <summary>
    /// 邮件地址（当MethodType=Email时使用）
    /// </summary>
    public string? EmailAddress { get; set; }
}

/// <summary>
/// 创建通知方式配置DTO
/// </summary>
public class CreateNotificationMethodConfigDto
{
    /// <summary>
    /// 通知方式类型
    /// </summary>
    public NotificationMethodType MethodType { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

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
}

/// <summary>
/// 查询参数
/// </summary>
public class NotificationBusinessQueryDto : PagedResultRequestDto
{
    /// <summary>
    /// 搜索关键词（业务名称或编码）
    /// </summary>
    public string? Filter { get; set; }
}
