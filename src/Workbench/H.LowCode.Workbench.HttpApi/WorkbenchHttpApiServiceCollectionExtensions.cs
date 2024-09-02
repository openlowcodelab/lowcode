using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.Workbench.HttpApi;

public static class WorkbenchHttpApiServiceCollectionExtensions
{
    public static void AddWorkbenchHttpApi(this IServiceCollection services)
    {
        services.AddApplication<WorkbenchHttpApiModule>();
    }
}
