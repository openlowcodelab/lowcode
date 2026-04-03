using H.Organization.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;

namespace H.Organization.HttpApi;

[DependsOn(
    typeof(OrganizationApplicationModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAutofacModule)
)]
public class OrganizationHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置控制器
        context.Services.AddControllers()
            .AddApplicationPart(typeof(OrganizationHttpApiModule).Assembly);
    }
}
