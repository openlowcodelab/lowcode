using H.LowCode.Workbench.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Workbench.HttpApi;

[DependsOn(typeof(AdminApplicationModule))]
internal class AdminHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddControllers().AddApplicationPart(typeof(AdminHttpApiModule).Assembly);
    }
}
