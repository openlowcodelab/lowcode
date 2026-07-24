using H.Notification.Application.Contracts;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace H.Notification.Application.Sending;

/// <summary>
/// 短信发送器（桩实现）：当前无真实网关，记录日志并视为成功。
/// 预留 provider 扩展点，后续可接入阿里云/腾讯云等短信服务。
/// </summary>
public class SmsSender : IChannelSender, ITransientDependency
{
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(ILogger<SmsSender> logger)
    {
        _logger = logger;
    }

    public NotificationChannelType Channel => NotificationChannelType.Sms;

    public Task<SendResult> SendAsync(NotificationDeliveryContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.Address))
        {
            return Task.FromResult(SendResult.Fail("通知人未配置手机号"));
        }

        _logger.LogInformation("[短信桩] 发送至 {Phone}: {Title} - {Content}", ctx.Address, ctx.Title, ctx.Content);
        return Task.FromResult(SendResult.Ok());
    }
}
