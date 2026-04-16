using H.Account.Application.Contracts;
using H.Admin.AppDrawer;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.Workbench;
using H.Organization.Application.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.LowCode.Host.All.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacWebAssemblyModule),
    //动态API代理
    typeof(AbpHttpClientModule),
    //DesignEngine
    typeof(LowCodeWorkbenchModule)
)]
public class HostAllClientModule : AbpModule
{
    public const string DesignEngineRemoteServiceName = "DesignEngine";
    public const string AccountRemoteServiceName = "Account";
    public const string OrganizationRemoteServiceName = "Organization";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        ConfigureHttpClient(context, environment);
        ConfigureHttpClientProxies(context);

        // 注册应用状态管理器
        context.Services.AddSingleton<AppStateManager>();

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(true));
    }

    private static void ConfigureHttpClient(ServiceConfigurationContext context, IWebAssemblyHostEnvironment environment)
    {
        // 注册 HTTP 客户端服务
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
            DesignEngineRemoteServiceName
        );

        // 注册 HTTP Client 代理
        context.Services.AddHttpClientProxies(
            typeof(AccountApplicationContractsModule).Assembly,
            AccountRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(OrganizationApplicationContractsModule).Assembly,
            OrganizationRemoteServiceName
        );
    }
}
