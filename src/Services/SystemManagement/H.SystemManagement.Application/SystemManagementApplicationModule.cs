using H.SystemManagement.Application.Contracts;
using H.SystemManagement.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;

namespace H.SystemManagement.Application;

[DependsOn(
    typeof(SystemManagementApplicationContractsModule),
    typeof(SystemManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementDomainModule),
    typeof(AbpAutoMapperModule)
)]
public class SystemManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();
        
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<SystemManagementApplicationModule>();
        });
    }
}
