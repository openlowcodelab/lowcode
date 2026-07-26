using H.Abstractions;

namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知分类管理接口
/// </summary>
public interface INotificationCategoryAppService : IAppService
{
    Task<List<NotificationCategoryDto>> GetAllAsync();
    Task<NotificationCategoryDto> GetAsync(long id);
    Task<NotificationCategoryDto> CreateAsync(CreateNotificationCategoryDto input);
    Task<NotificationCategoryDto> UpdateAsync(long id, UpdateNotificationCategoryDto input);
    Task DeleteAsync(long id);
}

/// <summary>
/// 联系人管理接口
/// </summary>
public interface IContactAppService : IAppService
{
    Task<PagedResultDto<ContactDto>> GetListAsync(ContactQueryDto input);
    Task<List<ContactDto>> GetAllEnabledAsync();
    Task<ContactDto> GetAsync(Guid id);
    Task<ContactDto> CreateAsync(CreateContactDto input);
    Task<ContactDto> UpdateAsync(Guid id, UpdateContactDto input);
    Task DeleteAsync(Guid id);
}

/// <summary>
/// 联系人分组管理接口
/// </summary>
public interface IContactGroupAppService : IAppService
{
    Task<PagedResultDto<ContactGroupDto>> GetListAsync(ContactGroupQueryDto input);
    Task<List<ContactGroupDto>> GetAllEnabledAsync();
    Task<ContactGroupDto> GetAsync(long id);
    Task<ContactGroupDto> CreateAsync(CreateContactGroupDto input);
    Task<ContactGroupDto> UpdateAsync(long id, UpdateContactGroupDto input);
    Task DeleteAsync(long id);
}

/// <summary>
/// 通知渠道管理接口
/// </summary>
public interface INotificationChannelAppService : IAppService
{
    Task<PagedResultDto<NotificationChannelDto>> GetListAsync(NotificationChannelQueryDto input);
    Task<List<NotificationChannelDto>> GetAllEnabledAsync();
    Task<NotificationChannelDto> GetAsync(Guid id);
    Task<NotificationChannelDto> CreateAsync(CreateNotificationChannelDto input);
    Task<NotificationChannelDto> UpdateAsync(Guid id, UpdateNotificationChannelDto input);
    Task DeleteAsync(Guid id);
}

/// <summary>
/// 通知业务管理接口
/// </summary>
public interface INotificationBusinessAppService : IAppService
{
    Task<PagedResultDto<NotificationBusinessDto>> GetListAsync(NotificationBusinessQueryDto input);
    Task<NotificationBusinessDto> GetAsync(Guid id);
    Task<NotificationBusinessDto> CreateAsync(CreateNotificationBusinessDto input);
    Task<NotificationBusinessDto> UpdateAsync(Guid id, UpdateNotificationBusinessDto input);
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 获取业务的通知规格（各级别渠道与阈值配置）
    /// </summary>
    Task<List<NotificationSpecDto>> GetSpecsAsync(Guid businessId);

    /// <summary>
    /// 设置业务的通知规则
    /// </summary>
    Task SetSpecsAsync(Guid businessId, List<NotificationSpecDto> specs);

    /// <summary>
    /// 获取业务绑定的联系人组ID
    /// </summary>
    Task<List<long>> GetGroupIdsAsync(Guid businessId);

    /// <summary>
    /// 设置业务绑定的联系人组
    /// </summary>
    Task SetGroupsAsync(Guid businessId, List<long> groupIds);
}

/// <summary>
/// 通知发送接口
/// </summary>
public interface INotificationSendAppService : IAppService
{
    Task<SendNotificationResult> SendAsync(SendNotificationInput input);
    Task<SendNotificationResult> TestSendAsync(TestSendInput input);
}

/// <summary>
/// 通知记录查询接口
/// </summary>
public interface INotificationRecordAppService : IAppService
{
    Task<PagedResultDto<NotificationRecordDto>> GetMasterListAsync(NotificationRecordQueryDto input);
    Task<PagedResultDto<InAppRecordDto>> GetInAppListAsync(ChannelRecordQueryDto input);
    Task<PagedResultDto<EmailRecordDto>> GetEmailListAsync(ChannelRecordQueryDto input);
    Task<PagedResultDto<SmsRecordDto>> GetSmsListAsync(ChannelRecordQueryDto input);
    Task<PagedResultDto<WebhookRecordDto>> GetWebhookListAsync(ChannelRecordQueryDto input);
}
