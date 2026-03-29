using Microsoft.Extensions.DependencyInjection;

namespace H.Account.Web;

/// <summary>
/// Web 层服务注册扩展
/// </summary>
public static class WebServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Web 层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAccountWeb(this IServiceCollection services)
    {
        services.AddAntDesign();

        return services;
    }
}
