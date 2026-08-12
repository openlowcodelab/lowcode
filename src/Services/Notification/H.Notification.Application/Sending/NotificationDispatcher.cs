using H.Notification.Application.Contracts;
using H.Notification.Application.Templates;
using H.Notification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace H.Notification.Application.Sending;

/// <summary>
/// 通知分发编排器：业务 -> 级别规格(渠道) -> 联系人组 -> 联系人 -> 渲染模板 -> 主记录+各渠道记录 -> 发送。
/// </summary>
public class NotificationDispatcher : ITransientDependency
{
    private readonly IRepository<NotificationBusinessEntity, Guid> _businessRepository;
    private readonly IRepository<NotificationChannelEntity, Guid> _channelRepository;
    private readonly IRepository<ContactEntity, Guid> _contactRepository;
    private readonly IRepository<ContactGroupMemberEntity, Guid> _memberRepository;
    private readonly IRepository<NotificationBusinessGroupEntity, Guid> _groupBindingRepository;
    private readonly IRepository<NotificationRecordEntity, Guid> _recordRepository;
    private readonly IEnumerable<IChannelSender> _senders;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        IRepository<NotificationBusinessEntity, Guid> businessRepository,
        IRepository<NotificationChannelEntity, Guid> channelRepository,
        IRepository<ContactEntity, Guid> contactRepository,
        IRepository<ContactGroupMemberEntity, Guid> memberRepository,
        IRepository<NotificationBusinessGroupEntity, Guid> groupBindingRepository,
        IRepository<NotificationRecordEntity, Guid> recordRepository,
        IEnumerable<IChannelSender> senders,
        IGuidGenerator guidGenerator,
        IClock clock,
        ILogger<NotificationDispatcher> logger)
    {
        _businessRepository = businessRepository;
        _channelRepository = channelRepository;
        _contactRepository = contactRepository;
        _memberRepository = memberRepository;
        _groupBindingRepository = groupBindingRepository;
        _recordRepository = recordRepository;
        _senders = senders;
        _guidGenerator = guidGenerator;
        _clock = clock;
        _logger = logger;
    }

    public async Task<SendNotificationResult> DispatchAsync(
        string businessCode,
        NotificationLevel? level,
        Dictionary<string, string> data,
        List<Guid>? contactIds,
        string? triggerSource)
    {
        var business = await (await _businessRepository.WithDetailsAsync(x => x.Specs, x => x.Templates))
            .FirstOrDefaultAsync(x => x.BusinessCode == businessCode);

        if (business == null)
        {
            return new SendNotificationResult { Message = $"未找到业务编码 {businessCode}" };
        }
        if (!business.IsEnabled)
        {
            return new SendNotificationResult { Message = $"业务 {business.BusinessName} 已禁用" };
        }

        var effectiveLevel = level ?? business.DefaultLevel;
        var spec = business.Specs.FirstOrDefault(s => s.Level == effectiveLevel && s.IsEnabled);
        if (spec == null)
        {
            return new SendNotificationResult { Message = $"业务 {business.BusinessName} 在级别 {effectiveLevel} 下未配置通知规则" };
        }

        var channelTypes = ParseChannels(spec.Channels);
        if (channelTypes.Count == 0)
        {
            return new SendNotificationResult { Message = $"业务 {business.BusinessName} 在级别 {effectiveLevel} 下未配置渠道" };
        }

        var contacts = await ResolveContactsAsync(business.Id, contactIds);
        if (contacts.Count == 0)
        {
            return new SendNotificationResult { Message = "未解析到可用联系人（未启用/未绑定联系人组）" };
        }

        // 渠道 provider 配置
        var channels = await (await _channelRepository.GetQueryableAsync()).Where(c => c.IsEnabled).ToListAsync();
        var channelConfigMap = channels.GroupBy(c => c.ChannelType).ToDictionary(g => g.Key, g => g.First().ConfigJson);

        var representativeTemplate = business.Templates.FirstOrDefault(t => t.IsEnabled);
        var record = new NotificationRecordEntity(_guidGenerator.Create())
        {
            BusinessId = business.Id,
            BusinessName = business.BusinessName,
            BusinessCode = business.BusinessCode,
            Level = effectiveLevel,
            Title = TemplateRenderer.Render(representativeTemplate?.Title ?? business.BusinessName, data),
            Content = TemplateRenderer.Render(representativeTemplate?.Content, data),
            DataJson = data.Count > 0 ? JsonSerializer.Serialize(data) : null,
            TriggerSource = triggerSource
        };

        var total = 0;
        var success = 0;
        var failed = 0;

        foreach (var channelType in channelTypes)
        {
            var template = business.Templates.FirstOrDefault(t => t.ChannelType == channelType && t.IsEnabled);
            var title = TemplateRenderer.Render(template?.Title ?? business.BusinessName, data);
            var content = TemplateRenderer.Render(template?.Content, data);
            channelConfigMap.TryGetValue(channelType, out var channelConfigJson);
            var sender = _senders.FirstOrDefault(s => s.Channel == channelType);

            foreach (var contact in contacts)
            {
                var address = ResolveAddress(channelType, contact);
                total++;

                var status = DeliveryStatus.Pending;
                string? error = null;
                DateTime? sentTime = null;

                if (string.IsNullOrWhiteSpace(address))
                {
                    status = DeliveryStatus.Failed;
                    error = "联系人未配置该渠道的目标地址";
                    failed++;
                }
                else if (sender == null)
                {
                    status = DeliveryStatus.Failed;
                    error = $"未找到 {channelType} 渠道发送器";
                    failed++;
                }
                else
                {
                    var result = await SafeSendAsync(sender, new NotificationDeliveryContext
                    {
                        ChannelType = channelType,
                        Address = address,
                        Title = title,
                        Content = content,
                        Level = effectiveLevel,
                        BusinessCode = business.BusinessCode,
                        ChannelConfigJson = channelConfigJson,
                        Data = data
                    });

                    if (result.Success)
                    {
                        status = DeliveryStatus.Sent;
                        sentTime = _clock.Now;
                        success++;
                    }
                    else
                    {
                        status = DeliveryStatus.Failed;
                        error = result.Error;
                        failed++;
                    }
                }

                AddChannelRecord(record, channelType, contact, address, title, content, status, error, sentTime);
            }
        }

        record.TotalCount = total;
        record.SuccessCount = success;
        record.FailedCount = failed;

        await _recordRepository.InsertAsync(record, autoSave: true);

        return new SendNotificationResult
        {
            MessageId = record.Id,
            TotalCount = total,
            SuccessCount = success,
            FailedCount = failed,
            Message = $"已处理 {total} 条投递，成功 {success} 条，失败 {failed} 条"
        };
    }

    private void AddChannelRecord(
        NotificationRecordEntity record,
        NotificationChannelType channelType,
        ContactEntity contact,
        string? address,
        string? title,
        string? content,
        DeliveryStatus status,
        string? error,
        DateTime? sentTime)
    {
        var now = _clock.Now;
        switch (channelType)
        {
            case NotificationChannelType.InApp:
                record.InAppRecords.Add(new InAppRecordEntity(_guidGenerator.Create())
                {
                    RecordId = record.Id,
                    Level = record.Level,
                    BusinessName = record.BusinessName,
                    ContactId = contact.Id,
                    ContactName = contact.Name,
                    Title = title,
                    Content = content,
                    Status = status,
                    ErrorMessage = error,
                    CreationTime = now,
                    SentTime = sentTime,
                    TargetUserId = contact.InAppUserId,
                    IsRead = false
                });
                break;
            case NotificationChannelType.Email:
                record.EmailRecords.Add(new EmailRecordEntity(_guidGenerator.Create())
                {
                    RecordId = record.Id,
                    Level = record.Level,
                    BusinessName = record.BusinessName,
                    ContactId = contact.Id,
                    ContactName = contact.Name,
                    Title = title,
                    Content = content,
                    Status = status,
                    ErrorMessage = error,
                    CreationTime = now,
                    SentTime = sentTime,
                    ToAddress = address
                });
                break;
            case NotificationChannelType.Sms:
                record.SmsRecords.Add(new SmsRecordEntity(_guidGenerator.Create())
                {
                    RecordId = record.Id,
                    Level = record.Level,
                    BusinessName = record.BusinessName,
                    ContactId = contact.Id,
                    ContactName = contact.Name,
                    Title = title,
                    Content = content,
                    Status = status,
                    ErrorMessage = error,
                    CreationTime = now,
                    SentTime = sentTime,
                    Phone = address
                });
                break;
            case NotificationChannelType.Webhook:
                record.WebhookRecords.Add(new WebhookRecordEntity(_guidGenerator.Create())
                {
                    RecordId = record.Id,
                    Level = record.Level,
                    BusinessName = record.BusinessName,
                    ContactId = contact.Id,
                    ContactName = contact.Name,
                    Title = title,
                    Content = content,
                    Status = status,
                    ErrorMessage = error,
                    CreationTime = now,
                    SentTime = sentTime,
                    Url = address
                });
                break;
        }
    }

    private async Task<List<ContactEntity>> ResolveContactsAsync(Guid businessId, List<Guid>? contactIds)
    {
        var contactQuery = await _contactRepository.GetQueryableAsync();

        if (contactIds != null && contactIds.Count > 0)
        {
            return await contactQuery.Where(c => contactIds.Contains(c.Id) && c.IsEnabled).ToListAsync();
        }

        var groupIds = await (await _groupBindingRepository.GetQueryableAsync())
            .Where(g => g.BusinessId == businessId).Select(g => g.GroupId).ToListAsync();
        if (groupIds.Count == 0)
        {
            return new List<ContactEntity>();
        }

        var memberContactIds = await (await _memberRepository.GetQueryableAsync())
            .Where(m => groupIds.Contains(m.GroupId)).Select(m => m.ContactId).Distinct().ToListAsync();
        if (memberContactIds.Count == 0)
        {
            return new List<ContactEntity>();
        }

        return await contactQuery.Where(c => memberContactIds.Contains(c.Id) && c.IsEnabled).ToListAsync();
    }

    private async Task<SendResult> SafeSendAsync(IChannelSender sender, NotificationDeliveryContext ctx)
    {
        try
        {
            return await sender.SendAsync(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "渠道 {Channel} 发送异常", ctx.ChannelType);
            return SendResult.Fail(ex.Message);
        }
    }

    private static List<NotificationChannelType> ParseChannels(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return new List<NotificationChannelType>();
        }
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => (NotificationChannelType)int.Parse(s.Trim()))
            .Distinct()
            .ToList();
    }

    private static string? ResolveAddress(NotificationChannelType channelType, ContactEntity contact)
    {
        return channelType switch
        {
            NotificationChannelType.InApp => contact.InAppUserId,
            NotificationChannelType.Email => contact.Email,
            NotificationChannelType.Sms => contact.Phone,
            NotificationChannelType.Webhook => contact.WebhookUrl,
            _ => null
        };
    }
}
