using H.LowCode.Designer.Application.Abstraction.AppServices;
using H.LowCode.Designer.Application.AppServices;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Designer.Application
{
    public class DesignerAppServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignerAppService, DesignerAppService>();
        }
    }
}
