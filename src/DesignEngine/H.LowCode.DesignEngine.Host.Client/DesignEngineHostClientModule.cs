using H.LowCode.ComponentBase;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.MyApp;
using H.LowCode.PartsDesignEngine;
using H.LowCode.Workbench;
using H.Util.Blazor;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Host.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacWebAssemblyModule),
    //动态API代理
    typeof(AbpHttpClientModule),
    typeof(DesignEngineApplicationContractsModule),
    //=====lowcode-web=====//
    //Workbench
    typeof(LowCodeWorkbenchModule),
    //DesignEngine
    typeof(DesignEngineModule),
    typeof(MyAppModule),
    typeof(PartsDesignEngineModule),
    //ComponentParts
    typeof(DefaultComponentModule),
    typeof(ExtensionComponentModule)
    )]
public class DesignEngineHostClientModule : AbpModule
{
    public const string RemoteServiceName = "Default";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureHttpClientProxies(context);

        //状态管理
        context.Services.AddScoped(typeof(ComponentStateWrapper<,>));

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(true));
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
