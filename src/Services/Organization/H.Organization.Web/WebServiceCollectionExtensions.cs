using H.Organization.Client;
using Microsoft.Extensions.DependencyInjection;

namespace H.Organization.Web;

/// <summary>
/// Web 层服务注册扩展
/// </summary>
public static class WebServiceCollectionExtensions
{
    /// <summary>
    /// 添加组织架构 Web 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="baseAddress">API 基础地址</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddOrganizationWebServices(
        this IServiceCollection services,
        string baseAddress)
    {
        services.AddOrganizationClient(baseAddress);

        return services;
    }
}
