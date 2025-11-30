using H.LowCode.Application.Contracts;

namespace H.LowCode.DesignEngine.Host.Client;

/// <summary>
/// AppId 请求头拦截器
/// 自动为 HTTP 请求添加 x-appid 请求头
/// </summary>
public class AppIdHeaderInterceptor : DelegatingHandler
{
    private readonly ICurrentApp _currentApp;
    private readonly ILogger<AppIdHeaderInterceptor> _logger;

    public AppIdHeaderInterceptor(
        ICurrentApp currentApp,
        ILogger<AppIdHeaderInterceptor> logger)
    {
        _currentApp = currentApp;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        try
        {
            // 获取当前 AppId
            _currentApp.ResolveAppIdFromContext();
            var appId = _currentApp.CurrentAppId;
            Console.WriteLine($"========= AppIdHeaderInterceptor =====: appId = {appId}, uri = {request.RequestUri}");

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