using H.Account.Application.Contracts;
using H.Account.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Identity;

namespace H.Account.Application;

[DependsOn(
    typeof(AccountEntityFrameworkCoreModule),
    typeof(AbpIdentityDomainModule)
)]
public class AccountApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 HttpContextAccessor
        context.Services.AddHttpContextAccessor();

        // 注册 HttpClient（供 WeChatAuthService / DingTalkAuthService 使用）
        context.Services.AddHttpClient();
    }
}
