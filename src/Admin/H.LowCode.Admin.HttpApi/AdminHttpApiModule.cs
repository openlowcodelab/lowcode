using H.LowCode.Admin.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Admin.HttpApi
{
    [DependsOn(typeof(ProAdminAppServiceModule))]
    internal class AdminHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddControllers().AddApplicationPart(typeof(AdminHttpApiModule).Assembly);
        }
    }
}
