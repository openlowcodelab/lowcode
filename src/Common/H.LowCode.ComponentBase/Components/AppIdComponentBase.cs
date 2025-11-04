using H.LowCode.Application.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace H.LowCode.ComponentBase.Components;

/// <summary>
/// 带有 AppId 支持的 Blazor 组件基类
/// 自动从应用上下文服务获取 AppId，并支持参数传递
/// </summary>
public abstract class AppIdComponentBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Inject] protected IAppContextService AppContextService { get; set; } = default!;
    [Inject] protected ILogger<AppIdComponentBase> Logger { get; set; } = default!;

    /// <summary>
    /// 应用 ID 参数
    /// 如果未通过参数传递，将自动从应用上下文服务获取
    /// </summary>
    [Parameter] public string? AppId { get; set; }

    /// <summary>
    /// 获取有效的 AppId
    /// 优先使用参数传递的 AppId，如果为空则从应用上下文服务获取
    /// </summary>
    protected string? EffectiveAppId
    {
        get
        {
            if (!string.IsNullOrEmpty(AppId))
            {
                return AppId;
            }

            var contextAppId = AppContextService.CurrentAppId;
            if (!string.IsNullOrEmpty(contextAppId))
            {
                return contextAppId;
            }

            return null;
        }
    }

    /// <summary>
    /// 组件初始化时的处理
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            // 如果没有通过参数传递 AppId，尝试从上下文获取
            if (string.IsNullOrEmpty(AppId))
            {
                var contextAppId = AppContextService.CurrentAppId;
                if (!string.IsNullOrEmpty(contextAppId))
                {
                    Logger.LogDebug("Using AppId from context: {AppId} for component: {ComponentType}", 
                        contextAppId, GetType().Name);
                }
                else
                {
                    Logger.LogWarning("No AppId available from context or parameters for component: {ComponentType}", 
                        GetType().Name);
                }
            }
            else
            {
                Logger.LogDebug("Using AppId from parameter: {AppId} for component: {ComponentType}", 
                    AppId, GetType().Name);
                
                // 如果通过参数传递了 AppId，更新应用上下文服务
                AppContextService.SetAppId(AppId);
            }

            await base.OnInitializedAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing component: {ComponentType}", GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// 确保 AppId 可用的辅助方法
    /// </summary>
    /// <returns>如果 AppId 可用返回 true，否则返回 false</returns>
    protected bool EnsureAppIdAvailable()
    {
        var appId = EffectiveAppId;
        if (string.IsNullOrEmpty(appId))
        {
            Logger.LogWarning("AppId is not available for component: {ComponentType}", GetType().Name);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 获取带有 AppId 的 API 路径
    /// </summary>
    /// <param name="apiPath">API 路径</param>
    /// <returns>带有 AppId 的完整 API 路径</returns>
    protected string GetApiPath(string apiPath)
    {
        var appId = EffectiveAppId;
        if (string.IsNullOrEmpty(appId))
        {
            Logger.LogWarning("AppId is not available, returning original API path: {ApiPath}", apiPath);
            return apiPath;
        }

        // 确保 API 路径以 / 开头
        if (!apiPath.StartsWith("/"))
        {
            apiPath = "/" + apiPath;
        }

        return $"/api/{appId}{apiPath}";
    }
}