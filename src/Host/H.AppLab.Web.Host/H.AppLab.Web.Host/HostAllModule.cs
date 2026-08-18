using H.Account.Application;
using H.Approval.Application;
using H.Assistant.Application;
using H.BackgroundTask.Application;
using H.Enterprise.Application;
using H.Enterprise.EntityFrameworkCore;
using H.File.Application;
using H.LowCode.ComponentBase;
using H.Util.Blazor;
using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using H.LowCode.RenderEngine.Application;
using H.LowCode.RenderEngine.EntityFrameworkCore;
using H.LowCode.RenderEngine.Repository.JsonFile;
using H.Mcp.YunXiao;
using H.Notification.Application;
using H.Order.Application;
using H.Organization.Application;
using H.Portal.Application;
using H.Setting.Application;
using H.SupplyChain.Application;
using H.SystemPortal.Application;
using H.Testing.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace H.AppLab.Web.Host;

/// <summary>
/// 统一宿主模块，整合所有应用模块
/// </summary>
[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
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
    // Testing
    typeof(TestingApplicationModule),
    // Portal
    typeof(PortalApplicationModule),
    // SystemPortal
    typeof(SystemPortalApplicationModule),
    // Notification
    typeof(NotificationApplicationModule),
    // Enterprise
    typeof(EnterpriseApplicationModule),
    // Order
    typeof(OrderApplicationModule),
    // Setting（配置管理）
    typeof(SettingApplicationModule),
    // SupplyChain
    typeof(SupplyChainApplicationModule),
    // BackgroundTask
    typeof(BackgroundTaskApplicationModule),
    // File（文件管理）
    typeof(FileApplicationModule),
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

        // 全局 Toast 提示服务
        context.Services.AddScoped<HToastService>();

        // 配置统一的 API 控制器
        ConfigureAutoApiControllers();

        // WASM 应用使用 JSON API + Cookie 认证，不需要服务端 CSRF 验证
        // SameSite Cookie 已提供足够的 CSRF 保护
        Configure<AbpAntiForgeryOptions>(options =>
        {
            options.AutoValidate = false;
        });
    }

    private const string SystemCookieScheme = "SystemCookies";

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/login";
                options.ExpireTimeSpan = TimeSpan.FromHours(24);
                options.SlidingExpiration = true;
            })
            .AddCookie(SystemCookieScheme, options =>
            {
                options.Cookie.Name = ".AspNetCore.SystemCookies";
                options.LoginPath = "/system/login";
                options.AccessDeniedPath = "/system/login";
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
            options.ConventionalControllers.Create(typeof(TestingApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(PortalApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(SystemPortalApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(NotificationApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(EnterpriseApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(OrderApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(SettingApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(SupplyChainApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(BackgroundTaskApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(FileApplicationModule).Assembly);
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

        // 从认证 Cookie 的 "TenantId" Claim 解析当前租户（企业选择后写入）
        // 置于解析器链首位，优先于 ABP 内置解析器生效
        Configure<AbpTenantResolveOptions>(options =>
        {
            options.TenantResolvers.Insert(0, new ClaimsTenantResolveContributor());
        });

        // 租户无效（如陈旧 __tenant Cookie 指向已删除的租户）时：
        // 清理 Cookie 并重定向到首页，而不是返回 404 阻断所有请求
        Configure<AbpAspNetCoreMultiTenancyOptions>(options =>
        {
            options.MultiTenancyMiddlewareErrorPageBuilder = async (httpContext, exception) =>
            {
                httpContext.Response.Cookies.Delete("__tenant");
                await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                httpContext.Response.Redirect("/");
                return true;
            };
        });
    }
}
