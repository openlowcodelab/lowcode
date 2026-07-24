using H.Notification.Application.Contracts;

namespace H.Notification.Application.Sending;

/// <summary>
/// 渠道发送上下文
/// </summary>
public class NotificationDeliveryContext
{
    public NotificationChannelType ChannelType { get; set; }

    /// <summary>
    /// 解析后的目标地址（邮箱/手机号/Webhook地址）
    /// </summary>
    public string? Address { get; set; }

    public string? Title { get; set; }
    public string? Content { get; set; }
    public NotificationLevel Level { get; set; }
    public string? BusinessCode { get; set; }

    /// <summary>
    /// 渠道配置（JSON）
    /// </summary>
    public string? ChannelConfigJson { get; set; }

    public Dictionary<string, string> Data { get; set; } = new();
}

/// <summary>
/// 发送结果
/// </summary>
public class SendResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }

    public static SendResult Ok() => new() { Success = true };
    public static SendResult Fail(string error) => new() { Success = false, Error = error };
}

/// <summary>
/// 渠道发送器接口
/// </summary>
public interface IChannelSender
{
    NotificationChannelType Channel { get; }

    Task<SendResult> SendAsync(NotificationDeliveryContext ctx);
}
