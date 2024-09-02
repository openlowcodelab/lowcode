using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.Workbench.HttpApi;

public static class AdminHttpApiServiceCollectionExtensions
{
    public static void AddAdminHttpApi(this IServiceCollection services)
    {
        services.AddApplication<AdminHttpApiModule>();
    }
}
