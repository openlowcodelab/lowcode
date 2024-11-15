using H.LowCode.ComponentBase;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.ThemeParts.AntBlazor;
using H.Util.Blazor;
using Volo.Abp.Autofac;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Host.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    //动态API代理
    typeof(AbpHttpClientModule),
    typeof(RenderEngineApplicationContractsModule),
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
    public const string RemoteServiceName = "Default";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //动态API代理
        context.Services.AddHttpClientProxies(
            typeof(RenderEngineApplicationContractsModule).Assembly,
            RemoteServiceName
        );

        //状态管理
        context.Services.AddScoped(typeof(ComponentStateWrapper<,>));

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(false));
    }
}
