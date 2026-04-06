using H.Account.Client;
using H.Organization.Client;
using Volo.Abp.Autofac.WebAssembly;
using Volo.Abp.Modularity;

namespace H.LowCode.Host.All.Client;

[DependsOn(
    typeof(AbpAutofacWebAssemblyModule),
    typeof(AccountClientModule),
    typeof(OrganizationClientModule)
)]
public class HostAllClientModule : AbpModule
{
}
