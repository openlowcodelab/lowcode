using H.Organization.Application.Contracts;
using H.Organization.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace H.Organization.Application;

[DependsOn(
    typeof(OrganizationEntityFrameworkCoreModule)
)]
public class OrganizationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

    }
}
