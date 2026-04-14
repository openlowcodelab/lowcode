using H.LowCode.Application;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Host.Client;
using H.LowCode.DesignEngine.Repository.JsonFile;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Host;

[DependsOn(
    //abp
    typeof(AbpAspNetCoreMvcModule),
    //Web
    typeof(DesignEngineHostClientModule),
    //Server
    typeof(LowCodeApplicationModule),
    typeof(DesignEngineApplicationModule),
    typeof(DesignEngineEntityFrameworkCoreModule),
    typeof(DesignEngineJsonFileRepositoryModule)
    )]
public class DesignEngineHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureAutoApiControllers();

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(true));
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
