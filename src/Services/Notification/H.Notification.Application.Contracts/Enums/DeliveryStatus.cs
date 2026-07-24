namespace H.Notification.Application.Contracts;

/// <summary>
/// 投递状态
/// </summary>
public enum DeliveryStatus
{
    /// <summary>
    /// 待发送
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已发送
    /// </summary>
    Sent = 1,

    /// <summary>
    /// 发送失败
    /// </summary>
    Failed = 2,

    /// <summary>
    /// 已读（仅站内信）
    /// </summary>
    Read = 3
}
