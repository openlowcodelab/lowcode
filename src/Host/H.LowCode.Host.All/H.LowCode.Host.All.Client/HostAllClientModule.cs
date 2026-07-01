using H.Account.Application.Contracts;
using H.Approval.Application.Contracts;
using H.Assistant.Application.Contracts;
using H.AutoTest.Application.Contracts;
using H.Enterprise.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.RenderEngine.Application.Contracts;
using H.Organization.Application.Contracts;
using H.Portal.Application.Contracts;
using H.SystemManagement.Application.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.LowCode.Host.All.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacWebAssemblyModule),
    //动态API代理
    typeof(AbpHttpClientModule)
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
    public const string SystemManagementRemoteServiceName = "SystemManagement";
    public const string AssistantRemoteServiceName = "Assistant";
    public const string EnterpriseRemoteServiceName = "Enterprise";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        ConfigureHttpClient(context, environment);
        ConfigureHttpClientProxies(context);

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(true));

        // AutoTest 测试执行事件通知器（WASM 端本地单例，避免 AppService 接口上的事件被 ABP ValidationInterceptor 反射崩溃）
        context.Services.AddSingleton<ITestExecutionEventNotifier, TestExecutionEventNotifier>();
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
            typeof(PortalApplicationContractsModule).Assembly,
            PortalRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(SystemManagementApplicationContractsModule).Assembly,
            SystemManagementRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(AssistantApplicationContractsModule).Assembly,
            AssistantRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(EnterpriseApplicationContractsModule).Assembly,
            EnterpriseRemoteServiceName
        );
    }
}
