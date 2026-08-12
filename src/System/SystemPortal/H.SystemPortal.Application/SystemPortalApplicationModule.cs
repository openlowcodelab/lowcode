using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.SystemPortal.Application;

public class SystemPortalApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<SystemUserStore>();
    }
}
