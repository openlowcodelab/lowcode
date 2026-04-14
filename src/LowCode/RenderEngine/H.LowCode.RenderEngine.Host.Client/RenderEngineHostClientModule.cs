using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.Components.Defaults;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngineBase;
using H.LowCode.Themes.AntBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Host.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacWebAssemblyModule),
    //动态API代理
    typeof(AbpHttpClientModule),
    //=====lowcode-web=====//
    typeof(RenderEngineBaseModule),
    typeof(RenderEngineApplicationContractsModule),
    //RenderEngine
    typeof(RenderEngineModule),
    //Components
    typeof(LowCodeDefaultComponentModule),
    //Themes
    typeof(AntBlazorThemeModule)
    )]
public class RenderEngineHostClientModule : AbpModule
{
    public const string RemoteServiceName = "Default";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        ConfigureHttpClient(context, environment);
        ConfigureHttpClientProxies(context);

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(false));
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
            typeof(RenderEngineApplicationContractsModule).Assembly,
            RemoteServiceName
        );
        context.Services.AddHttpClientProxies(
            typeof(LowCodeApplicationContractsModule).Assembly,
            RemoteServiceName
        );
    }
}
