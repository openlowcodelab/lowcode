using H.LowCode.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace H.LowCode.Application;

/// <summary>
/// AppId 请求头拦截器
/// 自动为 HTTP 请求添加 x-appid 请求头
/// </summary>
public class AppIdHeaderInterceptor : DelegatingHandler
{
    private readonly IAppContextService _appContextService;
    private readonly ILogger<AppIdHeaderInterceptor> _logger;

    public AppIdHeaderInterceptor(
        IAppContextService appContextService,
        ILogger<AppIdHeaderInterceptor> logger)
    {
        _appContextService = appContextService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // 获取当前 AppId
            var appId = _appContextService.CurrentAppId;
            
            if (!string.IsNullOrEmpty(appId))
            {
                // 如果请求头中还没有 x-appid，则添加
                if (!request.Headers.Contains("x-appid"))
                {
                    request.Headers.Add("x-appid", appId);
                    _logger.LogDebug("Added x-appid header: {AppId} to request: {RequestUri}", 
                        appId, request.RequestUri);
                }
                else
                {
                    _logger.LogDebug("x-appid header already exists in request: {RequestUri}", 
                        request.RequestUri);
                }
            }
            else
            {
                _logger.LogDebug("No AppId available to add to request header: {RequestUri}", 
                    request.RequestUri);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding x-appid header to request: {RequestUri}", 
                request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}