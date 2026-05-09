using H.Account.Application.Contracts;
using H.Admin.AppDrawer;
using H.Approval.Application.Contracts;
using H.AutoTest.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.Themes.AntBlazor;
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
    typeof(LowCodeWorkbenchModule),
    //RenderEngine
    typeof(AntBlazorThemeModule)
)]
public class HostAllClientModule : AbpModule
{
    public const string DesignEngineRemoteServiceName = "DesignEngine";
    public const string RenderEngineRemoteServiceName = "RenderEngine";
    public const string AccountRemoteServiceName = "Account";
    public const string OrganizationRemoteServiceName = "Organization";
    public const string ApprovalRemoteServiceName = "Approval";
    public const string AutoTestRemoteServiceName = "AutoTest";
    public const string PortalRemoteServiceName = "Portal";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        ConfigureHttpClient(context, environment);
        ConfigureHttpClientProxies(context);

        // 注册应用状态管理器
        context.Services.AddSingleton<AppStateManager>();

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(true));

        // AutoTest 测试执行事件通知器（WASM 端本地单例，避免 AppService 接口上的事件被 ABP ValidationInterceptor 反射崩溃）
        context.Services.AddSingleton<ITestExecutionEventNotifier, TestExecutionEventNotifier>();
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

        context.Services.AddHttpClientProxies(
            typeof(RenderEngineApplicationContractsModule).Assembly,
            RenderEngineRemoteServiceName
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
        
        context.Services.AddHttpClientProxies(
            typeof(ApprovalApplicationContractsModule).Assembly,
            ApprovalRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(AutoTestApplicationContractsModule).Assembly,
            AutoTestRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(H.Admin.Portal.IAppManageAppService).Assembly,
            PortalRemoteServiceName
        );
    }
}
