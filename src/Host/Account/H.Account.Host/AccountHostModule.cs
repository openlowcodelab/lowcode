using H.Account.Application;
using H.Admin.AppDrawer;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace H.Account.Host;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(AccountApplicationModule)
)]
public class AccountHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 AntDesign 服务
        context.Services.AddAntDesign();

        // 注册应用状态管理器
        context.Services.AddSingleton<AppStateManager>();

        ConfigureAutoApiControllers();
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AccountApplicationModule).Assembly);
        });
    }
}
