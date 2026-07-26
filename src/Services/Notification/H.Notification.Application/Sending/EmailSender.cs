using System.Net;
using System.Net.Mail;
using System.Text.Json;
using H.Notification.Application.Contracts;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace H.Notification.Application.Sending;

/// <summary>
/// 邮件渠道配置
/// </summary>
public class EmailChannelConfig
{
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? From { get; set; }
    public string? FromName { get; set; }
}

/// <summary>
/// 邮件发送器（SMTP）
/// </summary>
public class EmailSender : IChannelSender, ITransientDependency
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public NotificationChannelType Channel => NotificationChannelType.Email;

    public async Task<SendResult> SendAsync(NotificationDeliveryContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.Address))
        {
            return SendResult.Fail("通知人未配置邮箱地址");
        }

        if (string.IsNullOrWhiteSpace(ctx.ChannelConfigJson))
        {
            return SendResult.Fail("邮件渠道未配置 SMTP 参数");
        }

        EmailChannelConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<EmailChannelConfig>(ctx.ChannelConfigJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            return SendResult.Fail($"邮件渠道配置解析失败：{ex.Message}");
        }

        if (config == null || string.IsNullOrWhiteSpace(config.Host) || string.IsNullOrWhiteSpace(config.From))
        {
            return SendResult.Fail("邮件渠道配置缺少 Host 或 From");
        }

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(config.From, config.FromName ?? config.From),
                Subject = ctx.Title ?? string.Empty,
                Body = ctx.Content ?? string.Empty,
                IsBodyHtml = true
            };
            message.To.Add(ctx.Address);

            using var client = new SmtpClient(config.Host, config.Port)
            {
                EnableSsl = config.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(config.UserName))
            {
                client.Credentials = new NetworkCredential(config.UserName, config.Password);
            }

            await client.SendMailAsync(message);
            return SendResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "邮件发送失败: {Address}", ctx.Address);
            return SendResult.Fail($"邮件发送失败：{ex.Message}");
        }
    }
}
