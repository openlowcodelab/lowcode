using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.Organization.EntityFrameworkCore;

/// <summary>
/// EntityFrameworkCore 层服务注册扩展
/// </summary>
public static class EntityFrameworkCoreServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Organization DbContext 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddOrganizationDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "OrganizationDb")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        services.AddDbContext<OrganizationDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
