using H.Account.Application.Contracts;
using H.Admin.AppDrawer;
using H.Organization.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.Organization.Host;

/// <summary>
/// Organization Host 聚合模块，统一管理所有依赖
/// </summary>
[DependsOn(
    typeof(OrganizationApplicationModule),
    typeof(AbpHttpClientModule),
    typeof(AccountApplicationContractsModule)
)]
public class OrganizationHostModule : AbpModule
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
            options.ConventionalControllers.Create(typeof(OrganizationApplicationModule).Assembly);
        });
    }
}
