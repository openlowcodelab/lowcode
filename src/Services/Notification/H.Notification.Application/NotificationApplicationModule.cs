using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace H.Notification.Application;

[DependsOn(
    typeof(NotificationApplicationContractsModule),
    typeof(NotificationEntityFrameworkCoreModule),
    typeof(AbpAutoMapperModule)
)]
public class NotificationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();
        
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<NotificationApplicationModule>();
        });
    }
}
