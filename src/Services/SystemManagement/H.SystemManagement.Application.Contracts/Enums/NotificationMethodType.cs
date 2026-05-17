namespace H.SystemManagement.Application.Contracts.Enums;

/// <summary>
/// 通知方式类型
/// </summary>
public enum NotificationMethodType
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
