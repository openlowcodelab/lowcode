using H.Account.Application.Contracts;
using H.Account.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Identity;
using Volo.Abp.SettingManagement;

namespace H.Account.Application;

[DependsOn(
    typeof(AccountApplicationContractsModule),
    typeof(AccountEntityFrameworkCoreModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpSettingManagementDomainModule)
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
