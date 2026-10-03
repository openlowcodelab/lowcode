using H.Workbench.Core.Tools.Internal;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MailKit;
using MimeKit;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 通知技能：让员工能把结论推到群里/邮箱里，而不是只写在对话里等人截图。
/// 渠道走服务端登记名（与 DbTool 的数据源同一口径）：模型不给 URL、不给密码，
/// 未配置渠道时失败关闭。发消息是外溢动作，技能级默认要求人工批准。
/// </summary>
public class NotifyTool
{
    private const int MaxContentChars = 20000;
    private const int MaxRecipients = 20;

    private static readonly Regex AddressRegex = new(
        @"^[^@\s,;]+@[^@\s,;]+\.[^@\s,;]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private readonly WorkbenchToolOptions _options;
    private readonly ILogger<NotifyTool> _logger;

    public NotifyTool(IOptions<WorkbenchToolOptions> options, ILogger<NotifyTool> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    [Description("列出服务端已登记的通知渠道（只回名称与类型，不回地址与密钥）。发送前先调用它确认渠道名。")]
    public Task<string> NotifyListChannelsAsync(CancellationToken cancellationToken = default)
    {
        var channels = _options.Notify.Channels;
        if (channels.Count == 0)
        {
            return Task.FromResult(ToolEnvelope.Fail(
                "未登记任何通知渠道。请在服务端配置 Workbench:Notify:Channels 里添加渠道（Name/Kind/WebhookUrl 或邮箱参数）后重试"));
        }

        return Task.FromResult(ToolEnvelope.Ok(channels.Select(c => new
        {
            name = c.Name,
            kind = c.Kind,
            canSendIm = IsWebhookKind(c.Kind),
            canSendMail = string.Equals(c.Kind, "email", StringComparison.OrdinalIgnoreCase),
            canReadMail = string.Equals(c.Kind, "email", StringComparison.OrdinalIgnoreCase)
                          && !string.IsNullOrWhiteSpace(c.ImapHost)
        })));
    }

    [Description("向已登记的 IM 群机器人推送消息（钉钉/飞书/企业微信，渠道类型由服务端登记决定）。参数：channel（NotifyListChannelsAsync 返回的渠道名）, title, content（支持 Markdown，长度上限 20000 字）。")]
    public async Task<string> NotifySendAsync(
        [Description("渠道登记名")] string channel,
        [Description("消息标题")] string title,
        [Description("消息正文，可用 Markdown")] string content,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveChannel(channel, out var config, out var error))
        {
            return ToolEnvelope.Fail(error!);
        }

        if (!IsWebhookKind(config!.Kind))
        {
            return ToolEnvelope.Fail($"渠道 {channel} 类型为 {config.Kind}，不是 IM 机器人渠道");
        }

        if ((content ?? "").Length > MaxContentChars)
        {
            return ToolEnvelope.Fail($"正文超过 {MaxContentChars} 字上限");
        }

        var kind = config.Kind.ToLowerInvariant();
        string payload;
        var url = config.WebhookUrl;

        if (kind == "feishu")
        {
            var body = new Dictionary<string, object?>
            {
                ["msg_type"] = "text",
                ["content"] = new { text = BuildPlainTitle(title, content) }
            };

            if (!string.IsNullOrWhiteSpace(config.Secret))
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                body["timestamp"] = timestamp.ToString();
                body["sign"] = FeishuSign(timestamp, config.Secret);
            }

            payload = JsonSerializer.Serialize(body);
        }
        else if (kind == "wecom")
        {
            payload = JsonSerializer.Serialize(new
            {
                msgtype = "markdown",
                markdown = new { content = BuildMarkedTitle(title, content) }
            });
        }
        else
        {
            url = SignDingTalk(config);
            payload = JsonSerializer.Serialize(new
            {
                msgtype = "markdown",
                markdown = new
                {
                    title = string.IsNullOrWhiteSpace(title) ? "数字员工通知" : title,
                    text = BuildMarkedTitle(title, content)
                }
            });
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            using var response = await Http.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            var (ok, platformError) = EvaluateWebhookResponse(body);
            if (!response.IsSuccessStatusCode || !ok)
            {
                _logger.LogWarning("IM 推送失败: channel={Channel}, status={Status}, body={Body}",
                    channel, (int)response.StatusCode, Truncate(body, 400));
                return ToolEnvelope.Fail($"推送失败（HTTP {(int)response.StatusCode}）：" +
                                         (platformError ?? Truncate(body, 300)));
            }

            return ToolEnvelope.Ok(new
            {
                channel,
                kind,
                title,
                contentChars = (content ?? "").Length,
                response = Truncate(body, 200)
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "IM 推送异常: channel={Channel}", channel);
            return ToolEnvelope.Fail($"推送异常: {ex.Message}");
        }
    }

    [Description("通过已登记的邮箱渠道发送邮件（SMTP 账号由服务端配置，模型不给密码）。参数：channel, to（收件人，多个用 ; 或 , 分隔，最多 20 个）, subject, body, cc（可空）, isHtml（默认纯文本）。")]
    public async Task<string> EmailSendAsync(
        [Description("email 类型渠道的登记名")] string channel,
        [Description("收件邮箱地址，多个用 ; 或 , 分隔")] string to,
        [Description("邮件主题")] string subject,
        [Description("邮件正文")] string body,
        [Description("抄送地址，可空")] string? cc = null,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveChannel(channel, out var config, out var error))
        {
            return ToolEnvelope.Fail(error!);
        }

        if (!string.Equals(config!.Kind, "email", StringComparison.OrdinalIgnoreCase))
        {
            return ToolEnvelope.Fail($"渠道 {channel} 类型为 {config.Kind}，不是邮箱渠道");
        }

        if (string.IsNullOrWhiteSpace(config.SmtpHost) || string.IsNullOrWhiteSpace(config.Password))
        {
            return ToolEnvelope.Fail($"渠道 {channel} 的 SMTP 参数不完整（需要 SmtpHost/UserName/Password）");
        }

        if ((body ?? "").Length > MaxContentChars)
        {
            return ToolEnvelope.Fail($"正文超过 {MaxContentChars} 字上限");
        }

        if (!TryParseAddresses(to, out var recipients, out var addressError))
        {
            return ToolEnvelope.Fail(addressError!);
        }

        List<string>? carbon = null;
        if (!string.IsNullOrWhiteSpace(cc))
        {
            if (!TryParseAddresses(cc, out carbon, out addressError))
            {
                return ToolEnvelope.Fail(addressError!);
            }
        }

        var message = new MimeMessage();
        var from = string.IsNullOrWhiteSpace(config.From) ? config.UserName : config.From;
        message.From.Add(new MailboxAddress(config.FromDisplayName ?? "", from));
        foreach (var address in recipients!)
        {
            message.To.Add(new MailboxAddress("", address));
        }

        if (carbon is { Count: > 0 })
        {
            foreach (var address in carbon)
            {
                message.Cc.Add(new MailboxAddress("", address));
            }
        }

        message.Subject = subject ?? "";
        message.Body = isHtml
            ? new TextPart("html") { Text = body ?? "" }
            : new TextPart("plain") { Text = body ?? "" };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(config.SmtpHost, config.SmtpPort, SecureSocketOptions.Auto, cancellationToken);
            await client.AuthenticateAsync(config.UserName, config.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "邮件发送失败: channel={Channel}", channel);
            return ToolEnvelope.Fail($"邮件发送失败: {ex.Message}");
        }

        return ToolEnvelope.Ok(new
        {
            channel,
            subject,
            recipients = recipients!.Count,
            ccCount = carbon?.Count ?? 0,
            isHtml
        });
    }

    [Description("读取已登记邮箱的最新来信（IMAP，只读，不改服务器状态）。参数：channel, maxCount（默认 10，最多 30）, keyword（按主题模糊筛选，可空）, bodyChars（每封正文预览字符数，默认 300）。")]
    public async Task<string> EmailReceiveAsync(
        [Description("email 类型渠道的登记名")] string channel,
        int maxCount = 10,
        [Description("主题关键字，留空取最新")] string? keyword = null,
        int bodyChars = 300,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveChannel(channel, out var config, out var error))
        {
            return ToolEnvelope.Fail(error!);
        }

        if (!string.Equals(config!.Kind, "email", StringComparison.OrdinalIgnoreCase))
        {
            return ToolEnvelope.Fail($"渠道 {channel} 类型为 {config.Kind}，不是邮箱渠道");
        }

        if (string.IsNullOrWhiteSpace(config.ImapHost))
        {
            return ToolEnvelope.Fail($"渠道 {channel} 未配置 ImapHost，只能发不能收");
        }

        var wanted = Math.Clamp(maxCount > 0 ? maxCount : 10, 1, 30);
        var preview = Math.Clamp(bodyChars > 0 ? bodyChars : 300, 50, 1500);
        var items = new List<object>();
        var unreadCount = 0;

        try
        {
            using var client = new ImapClient();
            await client.ConnectAsync(config.ImapHost, config.ImapPort, SecureSocketOptions.Auto, cancellationToken);
            await client.AuthenticateAsync(config.UserName, config.Password, cancellationToken);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            unreadCount = (await inbox.SearchAsync(SearchQuery.NotSeen, cancellationToken)).Count;

            var messages = new List<MimeMessage>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var uids = (await inbox.SearchAsync(SearchQuery.SubjectContains(keyword.Trim()), cancellationToken))
                    .ToList();
                foreach (var uid in uids.Skip(Math.Max(0, uids.Count - wanted)).Reverse())
                {
                    messages.Add(await inbox.GetMessageAsync(uid, cancellationToken));
                }
            }
            else
            {
                for (var index = inbox.Count - 1; index >= Math.Max(0, inbox.Count - wanted); index--)
                {
                    messages.Add(await inbox.GetMessageAsync(index, cancellationToken));
                }
            }

            foreach (var message in messages)
            {
                items.Add(new
                {
                    subject = message.Subject,
                    from = message.From.Mailboxes.FirstOrDefault()?.Address,
                    receivedAt = message.Date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                    hasAttachments = message.Attachments.Any(),
                    body = Truncate(message.TextBody ?? HtmlToText(message.HtmlBody), preview)
                });
            }

            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "邮件读取失败: channel={Channel}", channel);
            return ToolEnvelope.Fail($"邮件读取失败: {ex.Message}");
        }

        // 工具结果超过 4000 字会被轨迹列截断成半截 JSON，这里先自行收到预算内
        while (items.Count > 1 && JsonSerializer.Serialize(items).Length > 3000)
        {
            items.RemoveAt(items.Count - 1);
        }

        return ToolEnvelope.Ok(new
        {
            channel,
            folder = "INBOX",
            returned = items.Count,
            unreadCount,
            items
        });
    }

    private bool TryResolveChannel(string? name, out NotifyChannelOptions? config, out string? error)
    {
        config = null;
        error = null;

        if (_options.Notify.Channels.Count == 0)
        {
            error = "未登记任何通知渠道（服务端配置 Workbench:Notify:Channels）。先用 NotifyListChannelsAsync 确认，或让管理员配置渠道";
            return false;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            error = "channel 不能为空";
            return false;
        }

        config = _options.Notify.Channels.FirstOrDefault(c =>
            string.Equals(c.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (config is null)
        {
            error = $"渠道 {name} 未登记，可用渠道：" +
                    string.Join(", ", _options.Notify.Channels.Select(c => c.Name));
            return false;
        }

        if (IsWebhookKind(config.Kind) && string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            error = $"渠道 {config.Name} 未配置 WebhookUrl";
            return false;
        }

        return true;
    }

    private static bool IsWebhookKind(string? kind)
    {
        var k = (kind ?? "").ToLowerInvariant();
        return k is "dingtalk" or "feishu" or "wecom";
    }

    private static string BuildMarkedTitle(string? title, string? content)
    {
        if (string.IsNullOrWhiteSpace(title)) return content ?? "";
        return $"### {title}\n\n{content ?? ""}";
    }

    private static string BuildPlainTitle(string? title, string? content)
    {
        if (string.IsNullOrWhiteSpace(title)) return content ?? "";
        return $"{title}\n{content ?? ""}";
    }

    /// <summary>
    /// 钉钉加签：timestamp + "\n" + secret 作为被签名字符串、secret 作密钥做 HMAC-SHA256，
    /// 结果 base64 后再 urlencode 拼到 webhook 上。群机器人未开加签时原样返回。
    /// </summary>
    private static string SignDingTalk(NotifyChannelOptions config)
    {
        if (string.IsNullOrWhiteSpace(config.Secret)) return config.WebhookUrl;

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var stringToSign = timestamp + "\n" + config.Secret;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(config.Secret));
        var sign = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
        var separator = config.WebhookUrl.Contains('?') ? "&" : "?";

        return $"{config.WebhookUrl}{separator}timestamp={timestamp}&sign={Uri.EscapeDataString(sign)}";
    }

    /// <summary>
    /// 飞书签名：HMAC-SHA256(key = timestamp + "\n" + secret, data = "") 的 base64，随消息体一起提交
    /// </summary>
    private static string FeishuSign(long timestamp, string secret)
    {
        var key = Encoding.UTF8.GetBytes(timestamp + "\n" + secret);
        using var hmac = new HMACSHA256(key);
        return Convert.ToBase64String(hmac.ComputeHash(Array.Empty<byte>()));
    }

    private static (bool Ok, string? Error) EvaluateWebhookResponse(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return (true, null);

        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            foreach (var key in new[] { "errcode", "code", "StatusCode" })
            {
                if (root.TryGetProperty(key, out var value))
                {
                    var numeric = value.ValueKind switch
                    {
                        JsonValueKind.Number => value.GetInt32(),
                        JsonValueKind.String when int.TryParse(value.GetString(), out var parsed) => parsed,
                        _ => 0
                    };

                    if (numeric != 0)
                    {
                        var message = root.TryGetProperty("errmsg", out var e) ? e.GetString()
                            : root.TryGetProperty("msg", out var m) ? m.GetString()
                            : root.TryGetProperty("message", out var msg) ? msg.GetString()
                            : null;
                        return (false, $"平台返回错误码 {numeric}：{message}");
                    }
                }
            }

            return (true, null);
        }
        catch (JsonException)
        {
            // 非 JSON 响应：钉钉/飞书正常返回 JSON，这里按可疑处理但交回原文
            return (true, null);
        }
    }

    private static bool TryParseAddresses(string? raw, out List<string>? addresses, out string? error)
    {
        addresses = null;
        error = null;

        var parts = (raw ?? "").Split(new[] { ';', ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim().Trim('<', '>'))
            .Where(p => p.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (parts.Count == 0)
        {
            error = "收件人不能为空";
            return false;
        }

        if (parts.Count > MaxRecipients)
        {
            error = $"收件人超过 {MaxRecipients} 个上限";
            return false;
        }

        var bad = parts.FirstOrDefault(p => !AddressRegex.IsMatch(p));
        if (bad is not null)
        {
            error = $"邮箱地址不合法: {bad}";
            return false;
        }

        addresses = parts;
        return true;
    }

    private static string HtmlToText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html)) return "";
        var stripped = Regex.Replace(html, "<[^>]+>", " ");
        var decoded = System.Net.WebUtility.HtmlDecode(stripped);
        return Regex.Replace(decoded, @"\s{2,}", " ").Trim();
    }

    private static string Truncate(string text, int max)
    {
        if (string.IsNullOrEmpty(text)) return "";
        return text.Length <= max ? text : text[..max] + "...";
    }
}
