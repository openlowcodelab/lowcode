using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace H.LowCode.DbMigrator;

[DependsOn(
    typeof(DesignEngineJsonFileRepositoryModule),
    typeof(DesignEngineEntityFrameworkCoreModule)
    )]
public class LowCodeDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IDataSeeder, DataSeeder>();

        context.Services.AddTransient<IDbSchemaMigrator, EntityFrameworkCoreDbSchemaMigrator>();

        //使用 MigratorDbContext 而不是 DesignEngineDbContext 的原因为需要指定迁移程序集，但又不想在 DesignEngineDbContext 中指定迁移程序集。
        context.Services.AddDbContext<MigratorDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration().GetConnectionString("Default");
            string migrationAssembly = typeof(DesignEngineEntityFrameworkCoreModule).Namespace;
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationAssembly));
        });
    }
}
