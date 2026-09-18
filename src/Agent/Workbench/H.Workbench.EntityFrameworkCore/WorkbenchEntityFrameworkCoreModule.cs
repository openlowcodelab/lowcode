using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;

namespace H.Workbench.EntityFrameworkCore;

[DependsOn(
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule)
)]
public class WorkbenchEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<WorkbenchDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });
    }
}
