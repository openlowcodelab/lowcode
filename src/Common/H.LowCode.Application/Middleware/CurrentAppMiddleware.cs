using H.LowCode.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace H.LowCode.Application;

/// <summary>
/// 应用上下文中间件
/// 在每个请求开始时自动解析并设置当前应用的 AppId
/// </summary>
public class CurrentAppMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CurrentAppMiddleware> _logger;

    public CurrentAppMiddleware(RequestDelegate next, ILogger<CurrentAppMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentApp currentApp)
    {
        try
        {
            // 在请求开始时自动解析 AppId
            currentApp.ResolveAppIdFromContext();
            
            _logger.LogDebug("CurrentApp middleware processed request for path: {Path}, AppId: {AppId}", 
                context.Request.Path, currentApp.CurrentAppId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CurrentApp middleware");
        }

        // 继续处理请求
        await _next(context);
    }
}