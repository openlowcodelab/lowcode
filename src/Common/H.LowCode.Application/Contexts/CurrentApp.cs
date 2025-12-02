using H.LowCode.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace H.LowCode.Application;

/// <summary>
/// 应用上下文服务实现
/// 负责从 HTTP 上下文中解析和管理当前请求的 AppId
/// </summary>
public class CurrentApp : ICurrentApp
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentApp> _logger;
    private string? _currentAppId;

    public CurrentApp(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentApp> logger)
    {
        _httpContextAccessor = httpContextAccessor;
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
    /// 
    /// </summary>
    public void ResolveAppIdFromContext()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            _logger.LogWarning("HttpContext is null, cannot resolve AppId");
            return;
        }

        _currentAppId = session.GetString("appid");
    }
}