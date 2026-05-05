using H.Admin.AppDrawer;
using H.LowCode.ComponentBase;
using H.LowCode.RenderEngine.Application;
using H.LowCode.RenderEngine.EntityFrameworkCore;
using H.LowCode.RenderEngine.Repository.JsonFile;
using H.LowCode.Themes.AntBlazor;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Host;

[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    //=====lowcode-server=====//
    typeof(RenderEngineApplicationModule),
    typeof(RenderEngineEntityFrameworkCoreModule),
    typeof(RenderEngineJsonFileRepositoryModule),
    //=====lowcode-web=====//
    typeof(AntBlazorThemeModule)
    )]
public class RenderEngineHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 AntDesign 服务
        context.Services.AddAntDesign();

        // 注册应用状态管理器
        context.Services.AddSingleton<AppStateManager>();

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(false));

        ConfigureAutoApiControllers();
    }

    private void ConfigureAutoApiControllers()
    {
        //动态API注册
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(RenderEngineApplicationModule).Assembly);
        });
    }
}
