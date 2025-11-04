using H.LowCode.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace H.LowCode.Application;

/// <summary>
/// 应用上下文中间件
/// 在每个请求开始时自动解析并设置当前应用的 AppId
/// </summary>
public class AppContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AppContextMiddleware> _logger;

    public AppContextMiddleware(RequestDelegate next, ILogger<AppContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAppContextService appContextService)
    {
        try
        {
            // 在请求开始时自动解析 AppId
            appContextService.ResolveAppIdFromContext();
            
            _logger.LogDebug("AppContext middleware processed request for path: {Path}, AppId: {AppId}", 
                context.Request.Path, appContextService.CurrentAppId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AppContext middleware");
        }

        // 继续处理请求
        await _next(context);
    }
}