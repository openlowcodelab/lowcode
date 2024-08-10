using H.LowCode.DesignEngine.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Parts.ExtensionComponents
{
    public class ExtensionComponentModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IComponentProvider, ExtensionComponentProvider>();
            context.Services.AddScoped<IPageTemplateProvider, ExtensionPageTemplateProvider>();
        }
    }
}
