using H.Abp.HttpClientProxy;
using H.Account.Application.Contracts;

namespace H.Account.Host.Client;

/// <summary>
/// Account 客户端服务注册
/// </summary>
public static class ClientServices
{
    public const string AccountRemoteServiceName = "Account";

    public static void Configure(IServiceCollection services, IConfiguration configuration, string baseAddress)
    {
        services.AddRemoteServices(configuration);

        // 默认 HttpClient（组件直接注入 HttpClient 时使用，相对路径基于宿主地址）
        services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        });

        // 注册 HttpClient，并为命名客户端添加 CookieHandler，
        // 使 Blazor WASM 的 fetch 请求携带认证 Cookie
        services.AddHttpClient();
        services.AddTransient<CookieHandler>();
        services.AddHttpClient(AccountRemoteServiceName).AddHttpMessageHandler<CookieHandler>();

        services.AddHttpClientProxies(
            typeof(AccountApplicationContractsModule).Assembly,
            AccountRemoteServiceName
        );
    }
}
