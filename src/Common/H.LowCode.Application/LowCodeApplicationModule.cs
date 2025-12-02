using H.LowCode.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Application;

public class LowCodeApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<ICurrentApp, CurrentApp>();
    }
}
