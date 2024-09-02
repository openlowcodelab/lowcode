using H.LowCode.Workbench.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Workbench.HttpApi;

[DependsOn(typeof(WorkbenchApplicationModule))]
internal class WorkbenchHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddControllers().AddApplicationPart(typeof(WorkbenchHttpApiModule).Assembly);
    }
}
