using H.Account.Application.Contracts;
using H.Account.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.Account.Application;

/// <summary>
/// Application 层服务注册扩展
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Application 层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAccountApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        //注册 EntityFrameworkCore 服务
        services.AddAccountDbContext(configuration);

        // 注册应用服务
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
}
