using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.Workbench;
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
    //DesignEngine
    typeof(LowCodeWorkbenchModule)
    )]
public class DesignEngineHostClientModule : AbpModule
{
    public const string RemoteServiceName = "DesignEngine";

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
        // 注册 HTTP 上下文访问器
        context.Services.AddHttpContextAccessor();

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
