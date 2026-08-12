using H.Abp.Application.Contracts;
using H.AppDrawer.Components;

namespace H.SystemPortal.Application.Contracts;

/// <summary>
/// 应用管理服务接口（包含查询和管理操作）
/// </summary>
public interface IAppManageAppService : IAppService
{
    /// <summary>
    /// 获取所有应用分类
    /// </summary>
    Task<AppCategoryInfo[]> GetAllCategoriesAsync();

    /// <summary>
    /// 添加应用
    /// </summary>
    Task<bool> AddAppAsync(string categoryName, AppItemInfo app);

    /// <summary>
    /// 更新应用
    /// </summary>
    Task<bool> UpdateAppAsync(string appId, AppItemInfo updatedApp);

    /// <summary>
    /// 删除应用
    /// </summary>
    Task<bool> DeleteAppAsync(string appId);

    /// <summary>
    /// 添加分类
    /// </summary>
    Task<bool> AddCategoryAsync(string categoryName);

    /// <summary>
    /// 删除分类
    /// </summary>
    Task<bool> DeleteCategoryAsync(string categoryName);
}
