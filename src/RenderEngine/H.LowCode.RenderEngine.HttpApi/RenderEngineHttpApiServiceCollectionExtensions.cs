using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.RenderEngine.HttpApi;

public static class RenderEngineHttpApiServiceCollectionExtensions
{
    public static void AddRenderEngineHttpApi(this IServiceCollection services)
    {
        services.AddApplication<RenderEngineHttpApiModule>();
    }
}
