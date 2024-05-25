using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.DesignEngine.HttpApi
{
    public static class DesignEngineHttpApiServiceCollectionExtensions
    {
        public static void AddDesignEngineHttpApi(this IServiceCollection services)
        {
            services.AddApplication<DesignEngineHttpApiModule>();
        }
    }
}
