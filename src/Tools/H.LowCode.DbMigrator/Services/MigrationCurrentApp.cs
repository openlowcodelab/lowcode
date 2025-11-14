using H.LowCode.Application.Contracts;
using H.LowCode.DesignEngine.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace H.LowCode.DbMigrator.Services;

/// <summary>
/// 迁移工具专用的应用上下文服务
/// 支持遍历所有应用的 AppId 进行数据库迁移
/// </summary>
public class MigrationCurrentApp : ICurrentApp
{
    private readonly ILogger<MigrationCurrentApp> _logger;
    private readonly IAppApplicationService _appService;
    private string? _currentAppId;

    public MigrationCurrentApp(
        ILogger<MigrationCurrentApp> logger,
        IAppApplicationService appService)
    {
        _logger = logger;
        _appService = appService;
    }

    /// <summary>
    /// 获取当前设置的 AppId
    /// </summary>
    public string? CurrentAppId => _currentAppId;

    /// <summary>
    /// 设置当前的 AppId
    /// </summary>
    /// <param name="appId">应用ID</param>
    public void SetAppId(string? appId)
    {
        _currentAppId = appId;
        _logger.LogDebug("Migration context: AppId set to {AppId}", appId);
    }

    /// <summary>
    /// 迁移过程中不需要从上下文解析 AppId
    /// </summary>
    public void ResolveAppIdFromContext()
    {
        _logger.LogDebug("Migration context: AppId resolution ignored");
    }

    /// <summary>
    /// 获取所有应用的 AppId 列表
    /// </summary>
    /// <returns>应用 ID 列表</returns>
    public async Task<IList<string>> GetAllAppIdsAsync()
    {
        var apps = await _appService.GetAppsAsync();
        return [.. apps.Select(a => a.Id)];
    }

    /// <summary>
    /// 遍历所有应用执行指定操作
    /// </summary>
    /// <param name="action">要执行的操作</param>
    public async Task ForEachAppAsync(Func<string, Task> action)
    {
        var appIds = await GetAllAppIdsAsync();

        foreach (var appId in appIds)
        {
            // 设置当前 AppId
            SetAppId(appId);
            
            try
            {
                await action(appId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理应用 {AppId} 时发生错误", appId);
                throw;
            }
        }
        
        // 清空当前 AppId
        SetAppId(null);
    }
}