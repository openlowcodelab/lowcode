using H.LowCode.DesignEngine.Components.Custom.ComponentProviders;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Components.Custom
{
    public class DesignEngineCustomComponentModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IComponentProvider, CustomComponentProvider>();
        }
    }
}
