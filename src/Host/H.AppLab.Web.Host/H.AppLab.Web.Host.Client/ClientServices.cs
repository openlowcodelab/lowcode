using H.Abp.HttpClientProxy;
using H.Account.Application.Contracts;
using H.Approval.Application.Contracts;
using H.Assistant.Application.Contracts;
using H.BackgroundTask.Application.Contracts;
using H.Enterprise.Application.Contracts;
using H.File.Application.Contracts;
using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using H.Util.Blazor;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngineBase;
using H.Notification.Application.Contracts;
using H.Order.Application.Contracts;
using H.Organization.Application.Contracts;
using H.Setting.Application.Contracts;
using H.SupplyChain.Application.Contracts;
using H.SystemPortal.Application.Contracts;
using H.Testing.Application.Contracts;

namespace H.AppLab.Web.Host.Client;

/// <summary>
/// 客户端服务注册（替代原有 ABP 模块系统）
/// 启动时仅注册首页所需服务；懒加载模块的服务在路由导航时通过 <see cref="LazyModuleRegistry"/> 延迟注册
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
    public const string FileRemoteServiceName = "File";
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

        // 全局 Toast 提示服务
        services.AddScoped<HToastService>();

        // 注册 HttpClient，并为每个命名客户端添加 CookieHandler，
        // 使 Blazor WASM 的 fetch 请求携带认证 Cookie
        services.AddHttpClient();
        services.AddTransient<CookieHandler>();
        services.AddTransient<AppIdHeaderHandler>();

        string[] serviceNames =
        [
            DesignEngineRemoteServiceName, RenderEngineRemoteServiceName,
            AccountRemoteServiceName, OrganizationRemoteServiceName,
            ApprovalRemoteServiceName, TestingRemoteServiceName,
            PortalRemoteServiceName, NotificationRemoteServiceName,
            AssistantRemoteServiceName, EnterpriseRemoteServiceName,
            SystemPortalRemoteServiceName, OrderRemoteServiceName,
            SettingRemoteServiceName, SupplyChainRemoteServiceName,
            BackgroundTaskRemoteServiceName, FileRemoteServiceName
        ];

        foreach (var name in serviceNames)
        {
            services.AddHttpClient(name)
                .AddHttpMessageHandler<CookieHandler>()
                .AddHttpMessageHandler<AppIdHeaderHandler>();
        }

        // 首页（Portal）仅依赖默认 HttpClient（AppDrawer、企业选择/创建页均直接调用 API），
        // 无需在启动时注册任何业务代理；各模块 Contracts 代理均在导航时延迟注册
    }

    /// <summary>
    /// 路由首段 → 模块注册 key（多个路由共享同一模块注册时使用；未列出的路由直接以路由首段作为 key）
    /// </summary>
    private static readonly Dictionary<string, string[]> RouteModuleKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["designengine"] = ["lowcode-render", "lowcode-design"],
        ["app"] = ["lowcode-render"],
    };

    /// <summary>
    /// 懒加载模块的延迟服务注册表（key 为模块注册 key）。
    /// lambda 仅在对应程序集下载完成后执行，其中的 typeof 引用不会触发启动时下载。
    /// </summary>
    private static readonly Dictionary<string, Action<IServiceCollection, IServiceProvider>> LazyModuleRegistrations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["organization"] = (s, _) =>
            s.AddHttpClientProxies(typeof(OrganizationApplicationContractsModule).Assembly, OrganizationRemoteServiceName),
        ["approval"] = (s, _) =>
        {
            // Approval.Web 交叉引用了 Organization 的 Contracts
            s.AddHttpClientProxies(typeof(ApprovalApplicationContractsModule).Assembly, ApprovalRemoteServiceName);
            s.AddHttpClientProxies(typeof(OrganizationApplicationContractsModule).Assembly, OrganizationRemoteServiceName);
        },
        ["testing"] = (s, _) =>
        {
            s.AddHttpClientProxies(typeof(TestingApplicationContractsModule).Assembly, TestingRemoteServiceName);
            // Testing 测试执行事件通知器
            s.AddSingleton<ITestExecutionEventNotifier, TestExecutionEventNotifier>();
        },
        ["notification"] = (s, _) =>
            s.AddHttpClientProxies(typeof(NotificationApplicationContractsModule).Assembly, NotificationRemoteServiceName),
        ["order"] = (s, _) =>
            s.AddHttpClientProxies(typeof(OrderApplicationContractsModule).Assembly, OrderRemoteServiceName),
        ["setting"] = (s, _) =>
            s.AddHttpClientProxies(typeof(SettingApplicationContractsModule).Assembly, SettingRemoteServiceName),
        ["supply-chain"] = (s, _) =>
            s.AddHttpClientProxies(typeof(SupplyChainApplicationContractsModule).Assembly, SupplyChainRemoteServiceName),
        ["background-task"] = (s, _) =>
            s.AddHttpClientProxies(typeof(BackgroundTaskApplicationContractsModule).Assembly, BackgroundTaskRemoteServiceName),
        ["file"] = (s, _) =>
            s.AddHttpClientProxies(typeof(FileApplicationContractsModule).Assembly, FileRemoteServiceName),
        ["account"] = (s, _) =>
            s.AddHttpClientProxies(typeof(AccountApplicationContractsModule).Assembly, AccountRemoteServiceName),
        ["assistant"] = (s, _) =>
            s.AddHttpClientProxies(typeof(AssistantApplicationContractsModule).Assembly, AssistantRemoteServiceName),
        ["system"] = (s, _) =>
        {
            s.AddHttpClientProxies(typeof(SystemPortalApplicationContractsModule).Assembly, SystemPortalRemoteServiceName);
            // SystemPortal.Web 使用 Enterprise 代理（System 层内部依赖，合法）
            s.AddHttpClientProxies(typeof(EnterpriseApplicationContractsModule).Assembly, EnterpriseRemoteServiceName);
        },
        // 设计器与应用渲染共享的 LowCode 基础服务
        ["lowcode-render"] = (s, _) =>
        {
            s.AddHttpClientProxies(typeof(LowCodeApplicationContractsModule).Assembly, DesignEngineRemoteServiceName);
            s.AddHttpClientProxies(typeof(RenderEngineApplicationContractsModule).Assembly, RenderEngineRemoteServiceName);
            // 应用状态（设计时宿主为 true）
            s.AddSingleton(new LowCodeAppState(true));
            // RenderEngineBase 服务（List 数据操作管理器等）
            s.AddRenderEngineBase();
        },
        // 设计器专属服务
        ["lowcode-design"] = (s, _) =>
            s.AddHttpClientProxies(typeof(DesignEngineApplicationContractsModule).Assembly, DesignEngineRemoteServiceName),
    };

    /// <summary>
    /// 懒加载程序集下载完成后，将对应模块的服务注册到模块子容器（registry 内部按模块 key 去重）
    /// </summary>
    public static void RegisterLazyModule(LazyModuleRegistry registry, string routeSegment)
    {
        var moduleKeys = RouteModuleKeys.GetValueOrDefault(routeSegment) ?? [routeSegment];

        foreach (var moduleKey in moduleKeys)
        {
            if (!LazyModuleRegistrations.TryGetValue(moduleKey, out var configure))
            {
                continue;
            }

            registry.RegisterModule(moduleKey, (services, root) =>
            {
                // 转发根容器的基础服务，供代理工厂在模块子容器内解析
                services.AddSingleton(root.GetRequiredService<RemoteServiceOptions>());
                services.AddSingleton(root.GetRequiredService<IHttpClientFactory>());
                configure(services, root);
            });
        }
    }
}
