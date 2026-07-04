using H.Account.Application.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.Account.Client;

[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    typeof(AbpHttpClientModule),
    typeof(AccountApplicationContractsModule)
)]
public class AccountClientModule : AbpModule
{
    public const string RemoteServiceName = "Account";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureHttpClientProxies(context);
    }

    private void ConfigureHttpClientProxies(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(AccountApplicationContractsModule).Assembly,
            RemoteServiceName
        );
    }
}
