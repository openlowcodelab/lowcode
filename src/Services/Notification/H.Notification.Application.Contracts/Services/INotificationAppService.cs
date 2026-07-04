using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.Notification.Application.Contracts;

/// <summary>
/// 通知业务管理接口
/// </summary>
public interface INotificationBusinessAppService : IApplicationService
{
    /// <summary>
    /// 获取分页列表
    /// </summary>
    Task<PagedResultDto<NotificationBusinessDto>> GetListAsync(NotificationBusinessQueryDto input);

    /// <summary>
    /// 获取详情（包含通知方式配置）
    /// </summary>
    Task<NotificationBusinessDto> GetAsync(Guid id);

    /// <summary>
    /// 创建通知业务
    /// </summary>
    Task<NotificationBusinessDto> CreateAsync(CreateNotificationBusinessDto input);

    /// <summary>
    /// 更新通知业务
    /// </summary>
    Task<NotificationBusinessDto> UpdateAsync(Guid id, UpdateNotificationBusinessDto input);

    /// <summary>
    /// 删除通知业务
    /// </summary>
    Task DeleteAsync(Guid id);
}

/// <summary>
/// 通知方式配置管理接口
/// </summary>
public interface INotificationMethodConfigAppService : IApplicationService
{
    /// <summary>
    /// 获取指定业务的所有通知方式配置
    /// </summary>
    Task<List<NotificationMethodConfigDto>> GetByBusinessIdAsync(Guid businessId);

    /// <summary>
    /// 创建通知方式配置
    /// </summary>
    Task<NotificationMethodConfigDto> CreateAsync(Guid businessId, CreateNotificationMethodConfigDto input);

    /// <summary>
    /// 更新通知方式配置
    /// </summary>
    Task<NotificationMethodConfigDto> UpdateAsync(Guid id, CreateNotificationMethodConfigDto input);

    /// <summary>
    /// 删除通知方式配置
    /// </summary>
    Task DeleteAsync(Guid id);
}
