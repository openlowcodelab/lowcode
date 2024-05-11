using H.LowCode.Admin.HttpApi;
using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode
{
    public static class AdminHttpApiServiceCollectionExtensions
    {
        public static void AddLowCodeHttpApi(this IServiceCollection services)
        {
            services.AddApplication<AdminHttpApiModule>();
        }
    }
}
