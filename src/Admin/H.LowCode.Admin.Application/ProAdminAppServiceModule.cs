using H.LowCode.Admin.Application.Abstraction.AppServices;
using H.LowCode.Admin.Application.AppServices;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Admin.Application
{
    public class ProAdminAppServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignerAppService, DesignerAppService>();
        }
    }
}
