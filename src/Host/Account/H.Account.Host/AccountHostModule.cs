using H.Account.Application;
using H.Account.Application.Contracts;
using H.Account.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace H.Account.Host;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AccountApplicationModule),
    typeof(AccountEntityFrameworkCoreModule)
)]
public class AccountHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 HttpClient
        context.Services.AddHttpClient();

        // 注册 HttpContextAccessor（用于 Server 端 API 控制器访问 HttpContext）
        context.Services.AddHttpContextAccessor();

        context.Services.AddAntDesign();

        ConfigureAutoApiControllers();
        
        ConfigureAuthentication(context);
        ConfigureExternalLogin(context);
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AccountApplicationModule).Assembly);
        });
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
}
