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
        // зЂВс AntDesign ЗўЮё
        context.Services.AddAntDesign();

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
