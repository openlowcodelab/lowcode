using H.Account.Application.Contracts;
using H.Organization.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace H.Organization.Host;

/// <summary>
/// Organization Host 聚合模块，统一管理所有依赖
/// </summary>
[DependsOn(
    typeof(AbpAspNetCoreMvcModule),
    typeof(OrganizationApplicationModule),
    typeof(AccountApplicationContractsModule)
)]
public class OrganizationHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 AntDesign 服务
        context.Services.AddAntDesign();

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
