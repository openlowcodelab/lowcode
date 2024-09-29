using H.LowCode.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace H.LowCode.DbMigrator;

[DependsOn(
    typeof(LowCodeEntityFrameworkCoreModule)
    )]
public class LowCodeDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

    }
}
