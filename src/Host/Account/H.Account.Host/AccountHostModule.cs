using H.Account.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace H.Account.Host;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AccountApplicationModule)
)]
public class AccountHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 HttpClient
        context.Services.AddHttpClient();

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
