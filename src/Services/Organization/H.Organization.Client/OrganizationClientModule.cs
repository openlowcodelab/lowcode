using H.Organization.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;

namespace H.Organization.Client;

[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    typeof(AbpHttpClientModule),
    typeof(OrganizationApplicationContractsModule)
)]
public class OrganizationClientModule : AbpModule
{
    public const string RemoteServiceName = "Organization";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureHttpClientProxies(context);
    }

    private void ConfigureHttpClientProxies(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(OrganizationApplicationContractsModule).Assembly,
            RemoteServiceName
        );
    }
}
