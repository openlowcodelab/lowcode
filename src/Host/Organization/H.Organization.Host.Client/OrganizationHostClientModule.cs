using H.Admin.AppDrawer;
using H.Organization.Application.Contracts;
using H.Organization.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.Organization.Host.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacWebAssemblyModule),
    //动态API代理
    typeof(AbpHttpClientModule),
    //Web
    typeof(OrganizationWebModule)
)]
public class OrganizationHostClientModule : AbpModule
{
    public const string AccountRemoteServiceName = "Account";
    public const string OrganizationRemoteServiceName = "Organization";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        ConfigureHttpClient(context, environment);
        ConfigureHttpClientProxies(context);

        // 注册应用状态管理器
        context.Services.AddSingleton<AppStateManager>();
    }

    private static void ConfigureHttpClient(ServiceConfigurationContext context, IWebAssemblyHostEnvironment environment)
    {
        // 注册 HTTP 客户端服务
        //context.Services.AddHttpContextAccessor();

        context.Services.AddTransient(sp => new HttpClient
        {
            BaseAddress = new Uri(environment.BaseAddress)
        });
    }

    private void ConfigureHttpClientProxies(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(OrganizationApplicationContractsModule).Assembly,
            OrganizationRemoteServiceName
        );
    }
}