using H.Account.Application.Contracts;
using H.Approval.Application.Contracts;
using H.Assistant.Application.Contracts;
using H.Testing.Application.Contracts;
using H.Enterprise.Application.Contracts;
using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngineBase;
using H.Notification.Application.Contracts;
using H.Order.Application.Contracts;
using H.Setting.Application.Contracts;
using H.Organization.Application.Contracts;
using H.BackgroundTask.Application.Contracts;
using H.Portal.Application.Contracts;
using H.SupplyChain.Application.Contracts;
using H.SystemPortal.Application.Contracts;
using H.HttpClientProxy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.AppLab.Host.All.Client;

/// <summary>
/// 客户端服务注册（替代原有 ABP 模块系统）
/// </summary>
public static class ClientServices
{
    public const string ApprovalRemoteServiceName = "Approval";
    public const string AccountRemoteServiceName = "Account";
    public const string OrganizationRemoteServiceName = "Organization";
    public const string DesignEngineRemoteServiceName = "DesignEngine";
    public const string RenderEngineRemoteServiceName = "RenderEngine";
    public const string TestingRemoteServiceName = "Testing";
    public const string PortalRemoteServiceName = "Portal";
    public const string NotificationRemoteServiceName = "Notification";
    public const string OrderRemoteServiceName = "Order";
    public const string SettingRemoteServiceName = "Setting";
    public const string SupplyChainRemoteServiceName = "SupplyChain";
    public const string BackgroundTaskRemoteServiceName = "BackgroundTask";
    public const string EnterpriseRemoteServiceName = "Enterprise";
    public const string SystemPortalRemoteServiceName = "SystemPortal";
    public const string AssistantRemoteServiceName = "Assistant";

    public static void Configure(IServiceCollection services, IConfiguration configuration, string baseAddress)
    {
        // 加载远程服务配置
        services.AddRemoteServices(configuration);

        // 默认 HttpClient（组件直接注入 HttpClient 时使用，相对路径基于宿主地址）
        services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(baseAddress)
        });

        // 注册 HttpClient，并为每个命名客户端添加 CookieHandler，
        // 使 Blazor WASM 的 fetch 请求携带认证 Cookie
        services.AddHttpClient();
        services.AddTransient<CookieHandler>();

        string[] serviceNames =
        [
            DesignEngineRemoteServiceName, RenderEngineRemoteServiceName,
            AccountRemoteServiceName, OrganizationRemoteServiceName,
            ApprovalRemoteServiceName, TestingRemoteServiceName,
            PortalRemoteServiceName, NotificationRemoteServiceName,
            AssistantRemoteServiceName, EnterpriseRemoteServiceName,
            SystemPortalRemoteServiceName, OrderRemoteServiceName,
            SettingRemoteServiceName, SupplyChainRemoteServiceName,
            BackgroundTaskRemoteServiceName
        ];

        foreach (var name in serviceNames)
        {
            services.AddHttpClient(name).AddHttpMessageHandler<CookieHandler>();
        }

        // 注册动态 API 代理
        ConfigureHttpClientProxies(services);

        // 应用状态
        services.AddSingleton(new LowCodeAppState(true));

        // RenderEngineBase 服务（List 数据操作管理器等）
        services.AddRenderEngineBase();

        // Testing 测试执行事件通知器
        services.AddSingleton<ITestExecutionEventNotifier, TestExecutionEventNotifier>();
    }

    private static void ConfigureHttpClientProxies(IServiceCollection services)
    {
        services.AddHttpClientProxies(
            typeof(LowCodeApplicationContractsModule).Assembly,
            DesignEngineRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(DesignEngineApplicationContractsModule).Assembly,
            DesignEngineRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(RenderEngineApplicationContractsModule).Assembly,
            RenderEngineRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(AccountApplicationContractsModule).Assembly,
            AccountRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(OrganizationApplicationContractsModule).Assembly,
            OrganizationRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(ApprovalApplicationContractsModule).Assembly,
            ApprovalRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(TestingApplicationContractsModule).Assembly,
            TestingRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(PortalApplicationContractsModule).Assembly,
            PortalRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(NotificationApplicationContractsModule).Assembly,
            NotificationRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(AssistantApplicationContractsModule).Assembly,
            AssistantRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(EnterpriseApplicationContractsModule).Assembly,
            EnterpriseRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(SystemPortalApplicationContractsModule).Assembly,
            SystemPortalRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(OrderApplicationContractsModule).Assembly,
            OrderRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(SettingApplicationContractsModule).Assembly,
            SettingRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(SupplyChainApplicationContractsModule).Assembly,
            SupplyChainRemoteServiceName
        );

        services.AddHttpClientProxies(
            typeof(BackgroundTaskApplicationContractsModule).Assembly,
            BackgroundTaskRemoteServiceName
        );
    }
}
