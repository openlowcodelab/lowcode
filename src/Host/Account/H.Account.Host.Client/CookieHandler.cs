using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace H.Account.Host.Client;

/// <summary>
/// Blazor WASM 的 DelegatingHandler：让 fetch 请求携带凭据（Cookie），
/// 否则 WASM 客户端发出的 API 请求默认不带认证 Cookie
/// </summary>
public class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}
