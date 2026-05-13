using H.Account.Application;
using H.Account.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Identity.EntityFrameworkCore;

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

        context.Services.AddAntDesign();

        ConfigureAutoApiControllers();
        
        ConfigureExternalLogin(context);
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(AccountApplicationModule).Assembly);
        });
    }
    
    private void ConfigureExternalLogin(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        
        context.Services.AddAuthentication()
            .AddOAuth("WeChat", options =>
            {
                options.ClientId = configuration["ExternalLogin:WeChat:ClientId"] ?? "";
                options.ClientSecret = configuration["ExternalLogin:WeChat:ClientSecret"] ?? "";
                options.CallbackPath = new PathString(configuration["ExternalLogin:WeChat:CallbackPath"] ?? "/signin-wechat");
                options.AuthorizationEndpoint = "https://open.weixin.qq.com/connect/oauth2/authorize";
                options.TokenEndpoint = "https://api.weixin.qq.com/sns/oauth2/access_token";
                options.UserInformationEndpoint = "https://api.weixin.qq.com/sns/userinfo";
                
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "openid");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "nickname");
                options.ClaimActions.MapJsonKey("urn:wechat:headimgurl", "headimgurl");
                
                options.SaveTokens = true;
                
                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                        
                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();
                        
                        var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        context.RunClaimActions(user.RootElement);
                    }
                };
            })
            .AddOAuth("DingTalk", options =>
            {
                options.ClientId = configuration["ExternalLogin:DingTalk:ClientId"] ?? "";
                options.ClientSecret = configuration["ExternalLogin:DingTalk:ClientSecret"] ?? "";
                options.CallbackPath = new PathString(configuration["ExternalLogin:DingTalk:CallbackPath"] ?? "/signin-dingtalk");
                options.AuthorizationEndpoint = "https://oapi.dingtalk.com/connect/oauth2/sns_authorize";
                options.TokenEndpoint = "https://oapi.dingtalk.com/sns/get_persistent_code";
                options.UserInformationEndpoint = "https://oapi.dingtalk.com/sns/getuserinfo_bycode";
                
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "openid");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "nick");
                options.ClaimActions.MapJsonKey("urn:dingtalk:unionid", "unionid");
                
                options.SaveTokens = true;
            });
    }
}
