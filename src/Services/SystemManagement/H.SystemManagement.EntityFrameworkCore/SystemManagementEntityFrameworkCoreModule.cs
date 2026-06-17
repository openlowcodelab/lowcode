using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace H.SystemManagement.EntityFrameworkCore;

public class SystemManagementEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("SystemManagementDb");

        context.Services.AddAbpDbContext<SystemManagementDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });
        
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings["SystemManagementDb"] = connectionString;
        });
    }
}
