using H.Account.Application.Contracts;
using H.Organization.Application;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.Organization.Host;

/// <summary>
/// Organization Host 聚合模块，统一管理所有依赖
/// </summary>
[DependsOn(
    typeof(OrganizationApplicationModule),
    typeof(AbpHttpClientModule),
    typeof(AccountApplicationContractsModule)
)]
public class OrganizationHostModule : AbpModule
{
    public const string RemoteServiceName = "Account";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // 配置 Account 服务的远程客户端代理
        context.Services.AddHttpClientProxies(
            typeof(AccountApplicationContractsModule).Assembly,
            RemoteServiceName
        );

        // 配置远程服务地址（从配置读取）
        context.Services.Configure<AbpRemoteServiceOptions>(options =>
        {
            options.RemoteServices.Default = new RemoteServiceConfiguration
            {
                BaseUrl = configuration["RemoteServices:Default:BaseUrl"] ?? "https://localhost:5002"
            };
        });
    }
}
