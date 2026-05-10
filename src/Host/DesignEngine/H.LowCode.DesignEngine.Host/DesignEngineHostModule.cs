using H.AppDrawer.Components;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Host;

[DependsOn(
    //abp
    typeof(AbpAspNetCoreMvcModule),
    //Server
    typeof(DesignEngineApplicationModule),
    typeof(DesignEngineEntityFrameworkCoreModule),
    typeof(DesignEngineJsonFileRepositoryModule)
    )]
public class DesignEngineHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 AntDesign 服务
        context.Services.AddAntDesign();

        // 注册应用状态管理器
        context.Services.AddSingleton<AppStateManager>();

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(true));

        ConfigureAutoApiControllers();
    }

    private void ConfigureAutoApiControllers()
    {
        //动态API注册
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(DesignEngineApplicationModule).Assembly);
        });
    }
}
