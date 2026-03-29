using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.Account.EntityFrameworkCore;

/// <summary>
/// EntityFrameworkCore 层服务注册扩展
/// </summary>
public static class EntityFrameworkCoreServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Account DbContext 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="connectionStringName">连接字符串名称</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAccountDbContext(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "AccountDb")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        services.AddDbContext<AccountDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
