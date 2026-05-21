using H.Account.Application;
using H.Agent.Application;
using H.Approval.Application;
using H.AutoTest.Application;
using H.LowCode.ComponentBase;
using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using H.LowCode.RenderEngine.Application;
using H.LowCode.RenderEngine.EntityFrameworkCore;
using H.LowCode.RenderEngine.Repository.JsonFile;
using H.Organization.Application;
using H.Portal.Application;
using H.SystemManagement.Application;
using H.SystemManagement.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace H.LowCode.Host.All;

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
    // Agent
    typeof(AgentApplicationModule),
    // Organization
    typeof(OrganizationApplicationModule),
    // Approval
    typeof(ApprovalApplicationModule),
    // AutoTest
    typeof(AutoTestApplicationModule),
    // Portal
    typeof(PortalApplicationModule),
    // SystemManagement
    typeof(SystemManagementApplicationModule)
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

        // 注册 AntDesign 服务
        context.Services.AddAntDesign();

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
            options.ConventionalControllers.Create(typeof(AgentApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(OrganizationApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(DesignEngineApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(RenderEngineApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(ApprovalApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(AutoTestApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(PortalApplicationModule).Assembly);
            options.ConventionalControllers.Create(typeof(SystemManagementApplicationModule).Assembly);
        });
    }
}
