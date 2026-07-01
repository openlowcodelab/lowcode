using Volo.Abp.Application.Services;

namespace H.SystemManagement.Application.Contracts;

/// <summary>
/// 设置管理接口
/// </summary>
public interface ISettingAppService : IApplicationService
{
    /// <summary>
    /// 获取所有设置项（按分组）
    /// </summary>
    Task<List<SettingGroupDto>> GetAllAsync();

    /// <summary>
    /// 获取指定分组的设置项
    /// </summary>
    Task<List<SettingItemDto>> GetByGroupAsync(string groupName);

    /// <summary>
    /// 获取单个设置项的值
    /// </summary>
    Task<SettingItemDto> GetAsync(string name);

    /// <summary>
    /// 更新设置项的值
    /// </summary>
    Task UpdateAsync(UpdateSettingItemDto input);

    /// <summary>
    /// 批量更新设置项
    /// </summary>
    Task UpdateManyAsync(List<UpdateSettingItemDto> inputs);
}
