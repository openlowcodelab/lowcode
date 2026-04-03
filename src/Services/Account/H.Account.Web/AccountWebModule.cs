using H.Account.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Components.WebAssembly;
using Volo.Abp.Modularity;

namespace H.Account.Web;

[DependsOn(
    typeof(AbpAspNetCoreComponentsWebAssemblyModule),
    typeof(AccountClientModule)
)]
public class AccountWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        // 配置远程服务地址
        context.Services.AddTransient(sp => new HttpClient
        {
            BaseAddress = new Uri(environment.BaseAddress)
        });

        // 注册 AntDesign
        context.Services.AddAntDesign();
    }
}
