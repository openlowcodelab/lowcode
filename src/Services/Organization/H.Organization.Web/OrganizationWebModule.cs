using H.Organization.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Components.WebAssembly;
using Volo.Abp.Modularity;

namespace H.Organization.Web;

[DependsOn(
    typeof(AbpAspNetCoreComponentsWebAssemblyModule),
    typeof(OrganizationClientModule)
)]
public class OrganizationWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var environment = context.Services.GetSingletonInstance<IWebAssemblyHostEnvironment>();

        // 配置远程服务地址
        context.Services.AddTransient(sp => new System.Net.Http.HttpClient
        {
            BaseAddress = new System.Uri(environment.BaseAddress)
        });

        // 注册 AntDesign
        context.Services.AddAntDesign();
    }
}
