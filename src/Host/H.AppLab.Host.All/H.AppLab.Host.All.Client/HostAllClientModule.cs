using H.Account.Application.Contracts;
using H.Approval.Application.Contracts;
using H.Assistant.Application.Contracts;
using H.AutoTest.Application.Contracts;
using H.Enterprise.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.RenderEngine.Application.Contracts;
using H.Notification.Application.Contracts;
using H.Order.Application.Contracts;
using H.Organization.Application.Contracts;
using H.Portal.Application.Contracts;
using H.SupplyChain.Application.Contracts;
using H.SystemPortal.Application.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.AppLab.Host.All.Client;

[DependsOn(
    //abp
    typeof(AbpAutofacWebAssemblyModule),
    //动态API代理
    typeof(AbpHttpClientModule)
)]
public class HostAllClientModule : AbpModule
{
    public const string ApprovalRemoteServiceName = "Approval";
    public const string AccountRemoteServiceName = "Account";
    public const string OrganizationRemoteServiceName = "Organization";
    public const string DesignEngineRemoteServiceName = "DesignEngine";
    public const string RenderEngineRemoteServiceName = "RenderEngine";
    public const string AutoTestRemoteServiceName = "AutoTest";
    public const string PortalRemoteServiceName = "Portal";
    public const string NotificationRemoteServiceName = "Notification";
    public const string OrderRemoteServiceName = "Order";
    public const string SupplyChainRemoteServiceName = "SupplyChain";

    public const string EnterpriseRemoteServiceName = "Enterprise";
    public const string SystemPortalRemoteServiceName = "SystemPortal";

    public const string AssistantRemoteServiceName = "Assistant";

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
            typeof(NotificationApplicationContractsModule).Assembly,
            NotificationRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(AssistantApplicationContractsModule).Assembly,
            AssistantRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(EnterpriseApplicationContractsModule).Assembly,
            EnterpriseRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(SystemPortalApplicationContractsModule).Assembly,
            SystemPortalRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(OrderApplicationContractsModule).Assembly,
            OrderRemoteServiceName
        );

        context.Services.AddHttpClientProxies(
            typeof(SupplyChainApplicationContractsModule).Assembly,
            SupplyChainRemoteServiceName
        );
    }
}
