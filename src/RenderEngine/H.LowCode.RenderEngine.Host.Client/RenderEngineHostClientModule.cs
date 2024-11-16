using H.LowCode.ComponentBase;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.ThemeParts.AntBlazor;
using H.Util.Blazor;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Host.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacWebAssemblyModule),
    //动态API代理
    typeof(AbpHttpClientModule),
    typeof(RenderEngineApplicationContractsModule),
    //=====lowcode-web=====//
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
        ConfigureHttpClientProxies(context);

        //状态管理
        context.Services.AddScoped(typeof(ComponentStateWrapper<,>));

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(false));
    }

    private void ConfigureHttpClientProxies(ServiceConfigurationContext context)
    {
        //动态API代理
        context.Services.AddHttpClientProxies(
            typeof(DesignEngineApplicationContractsModule).Assembly,
            RemoteServiceName
        );
    }
}
