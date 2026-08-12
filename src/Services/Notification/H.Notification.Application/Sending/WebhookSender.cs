using H.Notification.Application.Contracts;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Volo.Abp.DependencyInjection;

namespace H.Notification.Application.Sending;

/// <summary>
/// Webhook渠道配置
/// </summary>
public class WebhookChannelConfig
{
    /// <summary>
    /// 自定义请求头
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
}

/// <summary>
/// Webhook发送器（HTTP POST）
/// </summary>
public class WebhookSender : IChannelSender, ITransientDependency
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookSender> _logger;

    public WebhookSender(IHttpClientFactory httpClientFactory, ILogger<WebhookSender> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public NotificationChannelType Channel => NotificationChannelType.Webhook;

    public async Task<SendResult> SendAsync(NotificationDeliveryContext ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.Address))
        {
            return SendResult.Fail("通知人未配置 Webhook 地址");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("NotificationWebhook");
            using var request = new HttpRequestMessage(HttpMethod.Post, ctx.Address)
            {
                Content = JsonContent.Create(new
                {
                    businessCode = ctx.BusinessCode,
                    level = ctx.Level.ToString(),
                    title = ctx.Title,
                    content = ctx.Content,
                    data = ctx.Data
                })
            };

            ApplyHeaders(request, ctx.ChannelConfigJson);

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                return SendResult.Fail($"Webhook 返回状态码 {(int)response.StatusCode}");
            }

            return SendResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook 发送失败: {Url}", ctx.Address);
            return SendResult.Fail($"Webhook 发送失败：{ex.Message}");
        }
    }

    private static void ApplyHeaders(HttpRequestMessage request, string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return;
        }

        try
        {
            var config = JsonSerializer.Deserialize<WebhookChannelConfig>(configJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (config?.Headers == null)
            {
                return;
            }

            foreach (var header in config.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
        catch
        {
            // 配置无法解析时忽略自定义头
        }
    }
}
