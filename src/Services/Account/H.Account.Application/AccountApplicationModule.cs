using H.Account.Application.Contracts;
using H.Account.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Account.Application;

[DependsOn(
    typeof(AccountApplicationContractsModule),
    typeof(AccountEntityFrameworkCoreModule)
)]
public class AccountApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

    }
}
