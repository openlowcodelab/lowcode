using H.LowCode.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Application;

[DependsOn(
    typeof(LowCodeApplicationContractsModule)
)]

public class LowCodeApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ICurrentApp, CurrentApp>();
    }
}
