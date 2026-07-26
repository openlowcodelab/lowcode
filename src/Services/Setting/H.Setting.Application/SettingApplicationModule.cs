using H.Setting.Application.Contracts;
using H.Setting.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace H.Setting.Application;

[DependsOn(
    typeof(SettingApplicationContractsModule),
    typeof(SettingEntityFrameworkCoreModule)
)]
public class SettingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}
