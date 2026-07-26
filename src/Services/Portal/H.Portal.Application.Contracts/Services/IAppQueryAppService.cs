using H.AppDrawer.Components;
using Volo.Abp.Application.Services;

namespace H.Portal.Application.Contracts;

/// <summary>
/// 应用查询服务接口（只读），用于应用抽屉等场景获取应用数据
/// </summary>
public interface IAppQueryAppService : IApplicationService
{
    /// <summary>
    /// 获取所有应用分类
    /// </summary>
    Task<AppCategoryInfo[]> GetAllCategoriesAsync();

    /// <summary>
    /// 获取所有应用（扁平化）
    /// </summary>
    Task<AppItemInfo[]> GetAllAppsAsync();

    /// <summary>
    /// 根据 ID 获取应用
    /// </summary>
    Task<AppItemInfo?> GetAppByIdAsync(string appId);
}
