using H.LowCode.DesignEngine.Abstraction;
using H.LowCode.DesignEngine.DefaultComponents.ComponentProviders;
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
