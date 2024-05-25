using H.LowCode.DesignEngine.Abstraction;
using H.LowCode.DesignEngine.ExtensionComponents.ComponentProviders;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.ExtensionComponents
{
    public class CustomComponentModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IPageTemplateProvider, ExtensionPageTemplateProvider>();
        }
    }
}
