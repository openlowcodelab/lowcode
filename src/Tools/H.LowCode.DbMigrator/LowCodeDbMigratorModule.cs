using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace H.LowCode.DbMigrator;

[DependsOn(
    typeof(DesignEngineJsonFileRepositoryModule),
    typeof(DesignEngineEntityFrameworkCoreModule),
    typeof(DesignEngineApplicationModule)
    )]
public class LowCodeDbMigratorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<IDataSeeder, DataSeeder>();

        context.Services.AddTransient<IDbSchemaMigrator, EntityFrameworkCoreDbSchemaMigrator>();

        // 注册迁移专用的应用上下文服务
        context.Services.AddScoped<MigrationCurrentApp>();

        //使用 MigratorDbContext 而不是 DesignEngineDbContext 的原因为需要指定迁移程序集，但又不想在 DesignEngineDbContext 中指定迁移程序集。
        context.Services.AddDbContext<MigratorDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration().GetConnectionString("Default");
            // Ensure migrations assembly points to this DbMigrator project so runtime can find generated migrations
            var migrationAssembly = typeof(LowCodeDbMigratorModule).Assembly.GetName().Name;
            options
                .UseSqlServer(connectionString, b => b.MigrationsAssembly(migrationAssembly))
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });
    }
}
