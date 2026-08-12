using H.Abp.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace H.Abp.HttpClientProxy;

/// <summary>
/// IServiceCollection 扩展方法，提供 HTTP 客户端代理注册功能
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 从 IConfiguration 的 "RemoteServices" 节点加载远程服务配置
    /// </summary>
    public static IServiceCollection AddRemoteServices(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new RemoteServiceOptions();
        var section = configuration.GetSection("RemoteServices");
        foreach (var child in section.GetChildren())
        {
            var baseUrl = child["BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                options.Configure(child.Key, baseUrl);
            }
        }
        services.AddSingleton(options);
        return services;
    }

    /// <summary>
    /// 扫描程序集中所有继承 IAppService 的接口，注册 HTTP 客户端代理实现
    /// </summary>
    public static IServiceCollection AddHttpClientProxies(this IServiceCollection services, Assembly assembly, string remoteServiceName)
    {
        var serviceInterfaces = assembly.GetTypes()
            .Where(t => t.IsInterface && !t.IsGenericType && typeof(IAppService).IsAssignableFrom(t) && t != typeof(IAppService));

        foreach (var serviceInterface in serviceInterfaces)
        {
            RegisterProxy(services, serviceInterface, remoteServiceName);
        }

        return services;
    }

    private static void RegisterProxy(IServiceCollection services, Type serviceInterface, string remoteServiceName)
    {
        services.AddScoped(serviceInterface, sp =>
        {
            var options = sp.GetRequiredService<RemoteServiceOptions>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(remoteServiceName);
            var baseUrl = options.GetBaseUrl(remoteServiceName);

            // 使用反射调用 HttpClientProxyInterceptor<T>.Create
            var interceptorType = typeof(HttpClientProxyInterceptor<>).MakeGenericType(serviceInterface);
            var createMethod = interceptorType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)!;
            return createMethod.Invoke(null, [httpClient, baseUrl])!;
        });
    }
}
