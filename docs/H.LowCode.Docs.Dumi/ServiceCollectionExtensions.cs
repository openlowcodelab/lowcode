using System.Reflection;
using H.LowCode.Docs.Dumi.Util;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLowCodeDumiDocs(this IServiceCollection services, Assembly docAssembly)
        {
            MenuItemInitializer.InitMenuItems(docAssembly);

            return services;
        }
    }
}