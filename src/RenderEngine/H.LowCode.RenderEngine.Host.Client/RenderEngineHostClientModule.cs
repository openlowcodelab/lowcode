using BruTile.Wms;
using H.LowCode.ComponentBase;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.ThemeParts.AntBlazor;
using H.Util.Blazor;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Host.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    //=====web=====//
    //RenderEngine
    typeof(RenderEngineModule),
    //ComponentParts
    typeof(DefaultComponentModule),
    typeof(ExtensionComponentModule),
    //ThemeParts
    typeof(AntBlazorThemeModule)
    )]
public class RenderEngineHostClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //状态管理
        context.Services.AddScoped(typeof(ComponentStateWrapper<,>));

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(false));
    }
}
