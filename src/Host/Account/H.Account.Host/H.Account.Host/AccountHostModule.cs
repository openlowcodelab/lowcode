using H.Account.Application;
using H.Account.Application.Contracts;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace H.Account.Host;

[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(AccountApplicationModule),
    typeof(AccountApplicationContractsModule)
)]
public class AccountHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
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
