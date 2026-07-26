namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知级别
/// </summary>
public enum NotificationLevel
{
    /// <summary>
    /// 普通
    /// </summary>
    Normal = 0,

    /// <summary>
    /// 重要
    /// </summary>
    Important = 1,

    /// <summary>
    /// 紧急
    /// </summary>
    Urgent = 2,

    /// <summary>
    /// 严重
    /// </summary>
    Critical = 3
}
