using H.LowCode.DesignEngine.Components.ComponentProviders;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Components
{
    [DependsOn(typeof(DesignEngineModule))]
    public class DesignEngineComponentModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IComponentProvider, BasicComponentProvider>();
            context.Services.AddScoped<IComponentProvider, SeniorComponentProvider>();
            context.Services.AddScoped<IComponentProvider, LayoutComponentProvider>();
        }
    }
}
