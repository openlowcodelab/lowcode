namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知渠道类型
/// </summary>
public enum NotificationChannelType
{
    /// <summary>
    /// 站内通知
    /// </summary>
    InApp = 0,

    /// <summary>
    /// 邮件通知
    /// </summary>
    Email = 1,

    /// <summary>
    /// 短信通知
    /// </summary>
    Sms = 2,

    /// <summary>
    /// Webhook通知
    /// </summary>
    Webhook = 3
}
