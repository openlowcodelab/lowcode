using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知分类管理接口
/// </summary>
public interface INotificationCategoryAppService : IAppService
{
    Task<BaseOutput<List<NotificationCategoryDto>>> GetAllAsync();
    Task<BaseOutput<NotificationCategoryDto>> GetAsync(long id);
    Task<BaseOutput<NotificationCategoryDto>> CreateAsync(CreateNotificationCategoryDto input);
    Task<BaseOutput<NotificationCategoryDto>> UpdateAsync(long id, UpdateNotificationCategoryDto input);
    Task<BaseOutput> DeleteAsync(long id);
}

/// <summary>
/// 联系人管理接口
/// </summary>
public interface IContactAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<ContactDto>>> GetListAsync(ContactQueryDto input);
    Task<BaseOutput<List<ContactDto>>> GetAllEnabledAsync();
    Task<BaseOutput<ContactDto>> GetAsync(Guid id);
    Task<BaseOutput<ContactDto>> CreateAsync(CreateContactDto input);
    Task<BaseOutput<ContactDto>> UpdateAsync(Guid id, UpdateContactDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
}

/// <summary>
/// 联系人分组管理接口
/// </summary>
public interface IContactGroupAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<ContactGroupDto>>> GetListAsync(ContactGroupQueryDto input);
    Task<BaseOutput<List<ContactGroupDto>>> GetAllEnabledAsync();
    Task<BaseOutput<ContactGroupDto>> GetAsync(long id);
    Task<BaseOutput<ContactGroupDto>> CreateAsync(CreateContactGroupDto input);
    Task<BaseOutput<ContactGroupDto>> UpdateAsync(long id, UpdateContactGroupDto input);
    Task<BaseOutput> DeleteAsync(long id);
}

/// <summary>
/// 通知渠道管理接口
/// </summary>
public interface INotificationChannelAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<NotificationChannelDto>>> GetListAsync(NotificationChannelQueryDto input);
    Task<BaseOutput<List<NotificationChannelDto>>> GetAllEnabledAsync();
    Task<BaseOutput<NotificationChannelDto>> GetAsync(Guid id);
    Task<BaseOutput<NotificationChannelDto>> CreateAsync(CreateNotificationChannelDto input);
    Task<BaseOutput<NotificationChannelDto>> UpdateAsync(Guid id, UpdateNotificationChannelDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
}

/// <summary>
/// 通知业务管理接口
/// </summary>
public interface INotificationBusinessAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<NotificationBusinessDto>>> GetListAsync(NotificationBusinessQueryDto input);
    Task<BaseOutput<NotificationBusinessDto>> GetAsync(Guid id);
    Task<BaseOutput<NotificationBusinessDto>> CreateAsync(CreateNotificationBusinessDto input);
    Task<BaseOutput<NotificationBusinessDto>> UpdateAsync(Guid id, UpdateNotificationBusinessDto input);
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>
    /// 获取业务的通知规格（各级别渠道与阈值配置）
    /// </summary>
    Task<BaseOutput<List<NotificationSpecDto>>> GetSpecsAsync(Guid businessId);

    /// <summary>
    /// 设置业务的通知规则
    /// </summary>
    Task<BaseOutput> SetSpecsAsync(Guid businessId, List<NotificationSpecDto> specs);

    /// <summary>
    /// 获取业务绑定的联系人组ID
    /// </summary>
    Task<BaseOutput<List<long>>> GetGroupIdsAsync(Guid businessId);

    /// <summary>
    /// 设置业务绑定的联系人组
    /// </summary>
    Task<BaseOutput> SetGroupsAsync(Guid businessId, List<long> groupIds);
}

/// <summary>
/// 通知发送接口
/// </summary>
public interface INotificationSendAppService : IAppService
{
    Task<BaseOutput<SendNotificationResult>> SendAsync(SendNotificationInput input);
    Task<BaseOutput<SendNotificationResult>> TestSendAsync(TestSendInput input);
}

/// <summary>
/// 通知记录查询接口
/// </summary>
public interface INotificationRecordAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<NotificationRecordDto>>> GetMasterListAsync(NotificationRecordQueryDto input);
    Task<BaseOutput<PagedResultDto<InAppRecordDto>>> GetInAppListAsync(ChannelRecordQueryDto input);
    Task<BaseOutput<PagedResultDto<EmailRecordDto>>> GetEmailListAsync(ChannelRecordQueryDto input);
    Task<BaseOutput<PagedResultDto<SmsRecordDto>>> GetSmsListAsync(ChannelRecordQueryDto input);
    Task<BaseOutput<PagedResultDto<WebhookRecordDto>>> GetWebhookListAsync(ChannelRecordQueryDto input);
}
