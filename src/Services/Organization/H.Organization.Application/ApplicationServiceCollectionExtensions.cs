using H.Organization.Application.Contracts;
using H.Organization.Application.Services;
using H.Organization.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.Organization.Application;

/// <summary>
/// Application 层服务注册扩展
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// 添加组织架构应用服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddOrganizationApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        //注册 EntityFrameworkCore 服务
        services.AddOrganizationDbContext(configuration);

        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IRoleService, RoleService>();

        return services;
    }
}
