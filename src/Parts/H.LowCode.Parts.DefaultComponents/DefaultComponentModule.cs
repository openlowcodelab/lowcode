using H.LowCode.DesignEngine;
using H.LowCode.DesignEngine.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Parts.DefaultComponents
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
