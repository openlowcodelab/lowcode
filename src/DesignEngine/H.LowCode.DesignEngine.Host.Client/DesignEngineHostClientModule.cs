using H.LowCode.Application;
using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.Components.Defaults;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.MyApp;
using H.LowCode.PartsDesignEngine;
using H.LowCode.Portal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Host.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacWebAssemblyModule),
    //动态API代理
    typeof(AbpHttpClientModule),
    //=====lowcode-web=====//
    typeof(LowCodeApplicationModule),
    typeof(DesignEngineApplicationContractsModule),
    //Portal
    typeof(LowCodePortalModule),
    //DesignEngine
    typeof(DesignEngineModule),
    typeof(MyAppModule),
    typeof(PartsDesignEngineModule),
    //Components
    typeof(LowCodeDefaultComponentModule)
    )]
public class DesignEngineHostClientModule : AbpModule
{
    public const string RemoteServiceName = "Default";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        ConfigureHttpClient(context, environment);
        ConfigureHttpClientProxies(context);

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(true));
    }

    private static void ConfigureHttpClient(ServiceConfigurationContext context, IWebAssemblyHostEnvironment environment)
    {
        context.Services.AddTransient(sp => new HttpClient
        {
            BaseAddress = new Uri(environment.BaseAddress)
        });
    }

    private void ConfigureHttpClientProxies(ServiceConfigurationContext context)
    {
        //动态API代理
        context.Services.AddHttpClientProxies(
            typeof(DesignEngineApplicationContractsModule).Assembly,
            RemoteServiceName
        );
        context.Services.AddHttpClientProxies(
            typeof(LowCodeApplicationContractsModule).Assembly,
            RemoteServiceName
        );
    }
}
