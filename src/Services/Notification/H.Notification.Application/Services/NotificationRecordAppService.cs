using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Notification.Application;

public class NotificationRecordAppService : ApplicationService, INotificationRecordAppService
{
    private readonly IRepository<NotificationRecordEntity, Guid> _recordRepository;
    private readonly IRepository<InAppRecordEntity, Guid> _inAppRepository;
    private readonly IRepository<EmailRecordEntity, Guid> _emailRepository;
    private readonly IRepository<SmsRecordEntity, Guid> _smsRepository;
    private readonly IRepository<WebhookRecordEntity, Guid> _webhookRepository;

    public NotificationRecordAppService(
        IRepository<NotificationRecordEntity, Guid> recordRepository,
        IRepository<InAppRecordEntity, Guid> inAppRepository,
        IRepository<EmailRecordEntity, Guid> emailRepository,
        IRepository<SmsRecordEntity, Guid> smsRepository,
        IRepository<WebhookRecordEntity, Guid> webhookRepository)
    {
        _recordRepository = recordRepository;
        _inAppRepository = inAppRepository;
        _emailRepository = emailRepository;
        _smsRepository = smsRepository;
        _webhookRepository = webhookRepository;
    }

    public async Task<PagedResultDto<NotificationRecordDto>> GetMasterListAsync(NotificationRecordQueryDto input)
    {
        var query = (await _recordRepository.GetQueryableAsync())
            .WhereIf(input.BusinessId.HasValue, x => x.BusinessId == input.BusinessId)
            .WhereIf(input.Level.HasValue, x => x.Level == input.Level);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = entities.Select(e => new NotificationRecordDto
        {
            Id = e.Id,
            BusinessId = e.BusinessId,
            BusinessName = e.BusinessName,
            BusinessCode = e.BusinessCode,
            Level = e.Level,
            Title = e.Title,
            Content = e.Content,
            TriggerSource = e.TriggerSource,
            TotalCount = e.TotalCount,
            SuccessCount = e.SuccessCount,
            FailedCount = e.FailedCount,
            CreationTime = e.CreationTime
        }).ToList();

        return new PagedResultDto<NotificationRecordDto>(totalCount, dtos);
    }

    public async Task<PagedResultDto<InAppRecordDto>> GetInAppListAsync(ChannelRecordQueryDto input)
    {
        var query = (await _inAppRepository.GetQueryableAsync())
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => (x.ContactName != null && x.ContactName.Contains(input.Filter!)) || (x.BusinessName != null && x.BusinessName.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = items.Select(x => new InAppRecordDto
        {
            Id = x.Id,
            RecordId = x.RecordId,
            BusinessName = x.BusinessName,
            Level = x.Level,
            ContactId = x.ContactId,
            ContactName = x.ContactName,
            Title = x.Title,
            Content = x.Content,
            Status = x.Status,
            ErrorMessage = x.ErrorMessage,
            CreationTime = x.CreationTime,
            SentTime = x.SentTime,
            TargetUserId = x.TargetUserId,
            IsRead = x.IsRead,
            ReadTime = x.ReadTime
        }).ToList();

        return new PagedResultDto<InAppRecordDto>(totalCount, dtos);
    }

    public async Task<PagedResultDto<EmailRecordDto>> GetEmailListAsync(ChannelRecordQueryDto input)
    {
        var query = (await _emailRepository.GetQueryableAsync())
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => (x.ContactName != null && x.ContactName.Contains(input.Filter!)) || (x.ToAddress != null && x.ToAddress.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = items.Select(x => new EmailRecordDto
        {
            Id = x.Id,
            RecordId = x.RecordId,
            BusinessName = x.BusinessName,
            Level = x.Level,
            ContactId = x.ContactId,
            ContactName = x.ContactName,
            Title = x.Title,
            Content = x.Content,
            Status = x.Status,
            ErrorMessage = x.ErrorMessage,
            CreationTime = x.CreationTime,
            SentTime = x.SentTime,
            ToAddress = x.ToAddress
        }).ToList();

        return new PagedResultDto<EmailRecordDto>(totalCount, dtos);
    }

    public async Task<PagedResultDto<SmsRecordDto>> GetSmsListAsync(ChannelRecordQueryDto input)
    {
        var query = (await _smsRepository.GetQueryableAsync())
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => (x.ContactName != null && x.ContactName.Contains(input.Filter!)) || (x.Phone != null && x.Phone.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = items.Select(x => new SmsRecordDto
        {
            Id = x.Id,
            RecordId = x.RecordId,
            BusinessName = x.BusinessName,
            Level = x.Level,
            ContactId = x.ContactId,
            ContactName = x.ContactName,
            Title = x.Title,
            Content = x.Content,
            Status = x.Status,
            ErrorMessage = x.ErrorMessage,
            CreationTime = x.CreationTime,
            SentTime = x.SentTime,
            Phone = x.Phone
        }).ToList();

        return new PagedResultDto<SmsRecordDto>(totalCount, dtos);
    }

    public async Task<PagedResultDto<WebhookRecordDto>> GetWebhookListAsync(ChannelRecordQueryDto input)
    {
        var query = (await _webhookRepository.GetQueryableAsync())
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => (x.ContactName != null && x.ContactName.Contains(input.Filter!)) || (x.Url != null && x.Url.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = items.Select(x => new WebhookRecordDto
        {
            Id = x.Id,
            RecordId = x.RecordId,
            BusinessName = x.BusinessName,
            Level = x.Level,
            ContactId = x.ContactId,
            ContactName = x.ContactName,
            Title = x.Title,
            Content = x.Content,
            Status = x.Status,
            ErrorMessage = x.ErrorMessage,
            CreationTime = x.CreationTime,
            SentTime = x.SentTime,
            Url = x.Url,
            HttpStatus = x.HttpStatus
        }).ToList();

        return new PagedResultDto<WebhookRecordDto>(totalCount, dtos);
    }
}
