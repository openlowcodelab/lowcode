using H.Account.Application.Contracts;
using H.HttpClientProxy;
using Microsoft.Extensions.DependencyInjection;

namespace H.Account.Client;

/// <summary>
/// Account 客户端代理注册
/// </summary>
public static class AccountClientModule
{
    public const string RemoteServiceName = "Account";

    public static IServiceCollection AddAccountClientProxies(this IServiceCollection services)
    {
        services.AddHttpClientProxies(
            typeof(AccountApplicationContractsModule).Assembly,
            RemoteServiceName
        );
        return services;
    }
}
