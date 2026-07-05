using H.Account.Application;
using H.Assistant.Application;
using H.Approval.Application;
using H.AutoTest.Application;
using H.Enterprise.Application;
using H.Enterprise.EntityFrameworkCore;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using H.LowCode.RenderEngine.Application;
using H.LowCode.RenderEngine.EntityFrameworkCore;
using H.LowCode.RenderEngine.Repository.JsonFile;
using H.Organization.Application;
using H.Portal.Application;
using H.SystemPortal.Application;
using H.Notification.Application;
using H.YunXiaoMcpServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace H.AppLab.Host.All;

/// <summary>
/// 统一宿主模块，整合所有应用模块
/// </summary>
[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    // DesignEngine
    typeof(DesignEngineApplicationModule),
    typeof(DesignEngineEntityFrameworkCoreModule),
    typeof(DesignEngineJsonFileRepositoryModule),
    // RenderEngine
    typeof(RenderEngineApplicationModule),
    typeof(RenderEngineEntityFrameworkCoreModule),
    typeof(RenderEngineJsonFileRepositoryModule),
    // Account
    typeof(AccountApplicationModule),
    // Assistant
    typeof(AssistantApplicationModule),
    // Organization
    typeof(OrganizationApplicationModule),
    // Approval
    typeof(ApprovalApplicationModule),
    // AutoTest
    typeof(AutoTestApplicationModule),
    // Portal
    typeof(PortalApplicationModule),
    // SystemPortal
    typeof(SystemPortalApplicationModule),
    // Notification
    typeof(NotificationApplicationModule),
    // Enterprise
    typeof(EnterpriseApplicationModule),
    // YunXiao MCP Server
    typeof(YunXiaoMcpServerModule)
)]
public class HostAllModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 HttpClient
        context.Services.AddHttpClient();

        // 注册 HttpContextAccessor
        context.Services.AddHttpContextAccessor();

        // 配置 Cookie 认证
        ConfigureAuthentication(context);

        // 配置多租户
        ConfigureMultiTenancy(context);

        // 配置外部登录
        ConfigureExternalLogin(context);


        // 注册 LowCodeAppState (设计时为 true)
        context.Services.AddScoped(sp => new LowCodeAppState(isDesign: true));

        // 配置统一的 API 控制器
        ConfigureAutoApiControllers();
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/login";
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
            });
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            // 注册所有模块的控制器
            options.ConventionalControllers.Create(typeof(AccountApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(AssistantApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(OrganizationApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(DesignEngineApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(RenderEngineApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(ApprovalApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(AutoTestApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(PortalApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(SystemPortalApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(NotificationApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(EnterpriseApplicationModule).Assembly);
        });
    }

    private void ConfigureExternalLogin(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // 绑定外部登录配置
        context.Services.Configure<ExternalLoginOptions>(
            configuration.GetSection("ExternalLogin"));

        // 注册外部登录服务
        context.Services.AddTransient<WeChatAuthService>();
        context.Services.AddTransient<DingTalkAuthService>();
    }

    private void ConfigureMultiTenancy(ServiceConfigurationContext context)
    {
        // 启用多租户
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = true;
        });

        // 注册自定义 ITenantStore（从 Enterprise 数据库读取租户配置）
        context.Services.AddTransient<ITenantStore, EnterpriseTenantStore>();
    }
}
