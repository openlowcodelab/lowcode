using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngineBase;
using H.Abp.HttpClientProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.RenderEngine.Host.Client;

/// <summary>
/// RenderEngine 客户端服务注册
/// </summary>
public static class ClientServices
{
    public const string RemoteServiceName = "RenderEngine";

    public static void Configure(IServiceCollection services, IConfiguration configuration, string baseAddress)
    {
        // 加载远程服务配置
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
        services.AddHttpClient(RemoteServiceName).AddHttpMessageHandler<CookieHandler>();

        // 注册动态 API 代理
        services.AddHttpClientProxies(
            typeof(RenderEngineApplicationContractsModule).Assembly,
            RemoteServiceName
        );
        services.AddHttpClientProxies(
            typeof(LowCodeApplicationContractsModule).Assembly,
            RemoteServiceName
        );

        // 应用状态
        services.AddSingleton(new LowCodeAppState(false));

        // RenderEngineBase 服务（List 数据操作管理器等）
        services.AddRenderEngineBase();
    }
}
