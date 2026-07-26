using H.Notification.Application.Contracts;
using Volo.Abp.DependencyInjection;

namespace H.Notification.Application.Sending;

/// <summary>
/// 站内信发送器：投递记录本身即站内信，落库后即视为送达。
/// </summary>
public class InAppSender : IChannelSender, ITransientDependency
{
    public NotificationChannelType Channel => NotificationChannelType.InApp;

    public Task<SendResult> SendAsync(NotificationDeliveryContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.Address))
        {
            return Task.FromResult(SendResult.Fail("通知人未配置站内信目标标识"));
        }

        // 站内信内容已随投递记录持久化，此处无需额外动作。
        return Task.FromResult(SendResult.Ok());
    }
}
