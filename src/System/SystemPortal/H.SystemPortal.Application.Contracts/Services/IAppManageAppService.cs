using H.Abp.Application.Contracts;
using H.AppDrawer.Components;
using H.Util.Base;

namespace H.SystemPortal.Application.Contracts;

/// <summary>
/// 应用管理服务接口（包含查询和管理操作）
/// </summary>
public interface IAppManageAppService : IAppService
{
    /// <summary>
    /// 获取所有应用分类
    /// </summary>
    Task<BaseOutput<AppCategoryInfo[]>> GetAllCategoriesAsync();

    /// <summary>
    /// 添加应用
    /// </summary>
    Task<BaseOutput<bool>> AddAppAsync(string categoryName, AppItemInfo app);

    /// <summary>
    /// 更新应用
    /// </summary>
    Task<BaseOutput<bool>> UpdateAppAsync(string appId, AppItemInfo updatedApp);

    /// <summary>
    /// 删除应用
    /// </summary>
    Task<BaseOutput<bool>> DeleteAppAsync(string appId);

    /// <summary>
    /// 添加分类
    /// </summary>
    Task<BaseOutput<bool>> AddCategoryAsync(string categoryName);

    /// <summary>
    /// 删除分类
    /// </summary>
    Task<BaseOutput<bool>> DeleteCategoryAsync(string categoryName);
}
