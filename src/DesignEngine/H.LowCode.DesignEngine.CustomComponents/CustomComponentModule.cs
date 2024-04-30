using H.LowCode.DesignEngine.CustomComponents.ComponentProviders;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.CustomComponents
{
    public class CustomComponentModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IComponentProvider, CustomComponentProvider>();
        }
    }
}
