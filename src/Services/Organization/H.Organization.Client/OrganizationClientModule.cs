using H.Organization.Application.Contracts;
using H.Abp.HttpClientProxy;
using Microsoft.Extensions.DependencyInjection;

namespace H.Organization.Client;

/// <summary>
/// Organization 客户端代理注册
/// </summary>
public static class OrganizationClientModule
{
    public const string RemoteServiceName = "Organization";

    public static IServiceCollection AddOrganizationClientProxies(this IServiceCollection services)
    {
        services.AddHttpClientProxies(
            typeof(OrganizationApplicationContractsModule).Assembly,
            RemoteServiceName
        );
        return services;
    }
}
