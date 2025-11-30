using H.LowCode.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace H.LowCode.RenderEngineBase;

/// <summary>
/// 应用上下文服务实现
/// 负责从 HTTP 上下文中解析和管理当前请求的 AppId
/// </summary>
public class RenderEngineCurrentApp : ICurrentApp
{
    private readonly ILogger<RenderEngineCurrentApp> _logger;
    private string? _currentAppId;

    public RenderEngineCurrentApp(
        ILogger<RenderEngineCurrentApp> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取当前请求的 AppId
    /// </summary>
    public string? CurrentAppId
    {
        get
        {
            if (_currentAppId == null)
            {
                ResolveAppIdFromContext();
            }
            return _currentAppId;
        }
    }

    /// <summary>
    /// 设置当前请求的 AppId
    /// </summary>
    /// <param name="appId">应用ID</param>
    public void SetAppId(string? appId)
    {
        _currentAppId = appId;
        _logger.LogDebug("AppId set to: {AppId}", appId);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResolveAppIdFromContext()
    {
        try
        {
            _currentAppId = "attendance";

            _logger.LogDebug("AppId could not be resolved from context");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving AppId from context");
        }
    }
}