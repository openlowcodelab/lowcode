using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using H.Organization.EntityFrameworkCore;
using H.Organization.Application;

namespace H.Organization.HttpApi;

/// <summary>
/// HttpApi 层服务注册扩展
/// </summary>
public static class HttpApiServiceCollectionExtensions
{
    /// <summary>
    /// 添加 HttpApi 层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="jwtKey">JWT 密钥</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddOrganizationHttpApi(
        this IServiceCollection services,
        IConfiguration configuration,
        string? jwtKey = null)
    {
        // 注册 Application 层服务
        services.AddOrganizationApplication(configuration);

        services.AddControllers()
            .AddApplicationPart(typeof(HttpApiServiceCollectionExtensions).Assembly);

        return services;
    }
}
