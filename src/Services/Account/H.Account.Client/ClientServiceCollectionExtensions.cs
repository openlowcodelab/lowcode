using H.Account.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.Account.Client;

/// <summary>
/// Client 层服务注册扩展
/// </summary>
public static class ClientServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Account Client 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="baseAddress">API 基础地址</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAccountClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseAddress = configuration["Sites:AccountUrl"];
        services.AddHttpClient<AccountClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });

        return services;
    }

    /// <summary>
    /// 添加 Account Client 服务（使用命名 HttpClient）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="clientName">HttpClient 名称</param>
    /// <param name="baseAddress">API 基础地址</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAccountClient(
        this IServiceCollection services,
        string clientName,
        string baseAddress)
    {
        services.AddHttpClient(clientName, client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        })
        .AddTypedClient(c => new AccountClient(c));

        return services;
    }
}
