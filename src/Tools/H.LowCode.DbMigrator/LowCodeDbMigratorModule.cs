using H.LowCode.EntityFrameworkCore;
using H.LowCode.Repository.JsonFile;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace H.LowCode.DbMigrator;

[DependsOn(
    typeof(MetaJsonFileRepositoryModule),
    typeof(LowCodeEntityFrameworkCoreModule)
    )]
public class LowCodeDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IDataSeeder, DataSeeder>();

        context.Services.AddTransient<IDbSchemaMigrator, EntityFrameworkCoreDbSchemaMigrator>();

        //使用 DbMigratorDbContext 而不是 LowCodeDbContext 的原因为需要指定迁移程序集，但又不想在 LowCodeDbContext 中指定迁移程序集。
        context.Services.AddDbContext<DbMigratorDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration().GetConnectionString("Default");
            string migrationAssembly = typeof(LowCodeEntityFrameworkCoreModule).Namespace;
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationAssembly));
        });
    }
}
