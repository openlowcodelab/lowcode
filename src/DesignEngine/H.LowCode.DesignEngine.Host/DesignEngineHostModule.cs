using H.LowCode.ComponentBase;
using H.LowCode.Components.Defaults;
using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using H.LowCode.MyApp;
using H.LowCode.PartsDesignEngine;
using H.LowCode.Workbench;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Host;

[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    //=====lowcode-server=====//
    typeof(DesignEngineApplicationModule),
    typeof(DesignEngineEntityFrameworkCoreModule),
    typeof(DesignEngineJsonFileRepositoryModule),
    //=====lowcode-web=====//
    //Workbench
    typeof(LowCodeWorkbenchModule),
    //DesignEngine
    typeof(DesignEngineModule),
    typeof(MyAppModule),
    typeof(PartsDesignEngineModule),
    //Components
    typeof(LowCodeDefaultComponentModule)
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
