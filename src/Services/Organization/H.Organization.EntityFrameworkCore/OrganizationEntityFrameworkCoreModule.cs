using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Organization.EntityFrameworkCore;

public class OrganizationEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("OrganizationDb");

        context.Services.AddDbContext<OrganizationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }
}
