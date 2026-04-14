using H.Account.HttpApi;
using H.Account.Application;
using H.Account.EntityFrameworkCore;
using H.Organization.HttpApi;
using H.Organization.Application;
using H.Organization.EntityFrameworkCore;
using H.LowCode.Application;
using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.RenderEngine.Application;
using H.LowCode.RenderEngine.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace H.LowCode.Host.All;

/// <summary>
/// 统一宿主模块，整合所有应用模块
/// </summary>
[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    // Account
    typeof(AccountHttpApiModule),
    // Organization
    typeof(OrganizationHttpApiModule),
    // LowCode Common
    typeof(LowCodeApplicationModule),
    // DesignEngine 模块
    typeof(DesignEngineApplicationModule),
    // RenderEngine 模块
    typeof(RenderEngineApplicationModule)
)]
public class HostAllModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置统一的 API 控制器
        ConfigureAutoApiControllers();
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // 注册所有模块的控制器
            options.ConventionalControllers.Create(typeof(AccountHttpApiModule).Assembly);
            options.ConventionalControllers.Create(typeof(OrganizationHttpApiModule).Assembly);
            options.ConventionalControllers.Create(typeof(H.LowCode.DesignEngine.Application.DesignEngineApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(H.LowCode.RenderEngine.Application.RenderEngineApplicationModule).Assembly);
        });
    }
}
