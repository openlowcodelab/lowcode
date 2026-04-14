using H.Account.Application;
using H.LowCode.Application;
using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using H.LowCode.Host.All.Client;
using H.LowCode.RenderEngine.Application;
using H.LowCode.RenderEngine.EntityFrameworkCore;
using H.LowCode.RenderEngine.Repository.JsonFile;
using H.Organization.Application;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace H.LowCode.Host.All;

/// <summary>
/// 统一宿主模块，整合所有应用模块
/// </summary>
[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    //Web（所有应用）
    typeof(HostAllClientModule),
    // Account
    typeof(AccountApplicationModule),
    // Organization
    typeof(OrganizationApplicationModule),
    // LowCode Common
    typeof(LowCodeApplicationModule),
    // DesignEngine
    typeof(DesignEngineApplicationModule),
    typeof(DesignEngineEntityFrameworkCoreModule),
    typeof(DesignEngineJsonFileRepositoryModule),
    // RenderEngine
    typeof(RenderEngineApplicationModule),
    typeof(RenderEngineEntityFrameworkCoreModule),
    typeof(RenderEngineJsonFileRepositoryModule)
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
            options.ConventionalControllers.Create(typeof(AccountApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(OrganizationApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(DesignEngineApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(RenderEngineApplicationModule).Assembly);
        });
    }
}
