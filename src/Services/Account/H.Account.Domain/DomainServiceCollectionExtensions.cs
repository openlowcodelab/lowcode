using Microsoft.Extensions.DependencyInjection;

namespace H.Account.Domain;

/// <summary>
/// Domain 层服务注册扩展
/// </summary>
public static class DomainServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Domain 层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAccountDomain(this IServiceCollection services)
    {
        // Domain 层主要是实体定义，通常不需要注册服务
        // 如果需要注册领域服务，可以在这里添加
        return services;
    }
}
