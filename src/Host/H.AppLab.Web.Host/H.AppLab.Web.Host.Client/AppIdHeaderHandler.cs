using Microsoft.AspNetCore.Components;

namespace H.AppLab.Web.Host.Client;

/// <summary>
/// 从当前路由解析低代码应用 Id（/app/{appId}/... 或 /designer/{appId}/...），
/// 并以 appid 请求头发送，供服务端动态实体模型按应用加载数据源
/// </summary>
public class AppIdHeaderHandler : DelegatingHandler
{
    private readonly NavigationManager _navigationManager;

    public AppIdHeaderHandler(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var appId = ResolveAppIdFromRoute();
        if (!string.IsNullOrEmpty(appId))
        {
            request.Headers.TryAddWithoutValidation("appid", appId);
        }

        return base.SendAsync(request, cancellationToken);
    }

    private string? ResolveAppIdFromRoute()
    {
        try
        {
            var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length >= 2
                && (string.Equals(segments[0], "app", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(segments[0], "designer", StringComparison.OrdinalIgnoreCase)))
            {
                return segments[1];
            }
        }
        catch
        {
            // 路由解析失败时不附加请求头
        }

        return null;
    }
}
