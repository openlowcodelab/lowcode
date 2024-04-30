using H.LowCode.DesignEngine.DefaultComponents.ComponentProviders;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.DefaultComponents
{
    [DependsOn(typeof(DesignEngineModule))]
    public class DefaultComponentModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IComponentProvider, BasicComponentProvider>();
            context.Services.AddScoped<IComponentProvider, SeniorComponentProvider>();
            context.Services.AddScoped<IComponentProvider, LayoutComponentProvider>();
        }
    }
}
