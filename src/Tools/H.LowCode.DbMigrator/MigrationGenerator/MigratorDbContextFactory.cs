using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.DesignEngine.Repository.JsonFile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.DbMigrator;

/// <summary>
/// 通过 dotnet ef 命令生成迁移脚本时, dotnet ef tools 会读取 IDesignTimeDbContextFactory<TContext> 实现类注册 DbContext
/// 命令: dotnet ef migrations add <MigrationName> --context MigratorDbContext  (注意: 需要指定 --context 为 MigratorDbContext)
/// </summary>
public class MigratorDbContextFactory : IDesignTimeDbContextFactory<MigratorDbContext>
{
    public MigratorDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        //指定迁移文件生成到当前程序集（DbMigrator）中，而不是主项目中
        string connectionString = configuration.GetConnectionString("LowCodeDb")!;
        var builder = new DbContextOptionsBuilder<MigratorDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        var services = new ServiceCollection();
        services.AddApplication<LowCodeDbMigratorModule>();
        services.AddApplication<DesignEngineJsonFileRepositoryModule>();
        var serviceProvider = services.BuildServiceProvider();
        EntityTypeManager entityTypeManager = serviceProvider.GetService<EntityTypeManager>();
        MigrationCurrentApp currentApp = serviceProvider.GetService<MigrationCurrentApp>();

        return new MigratorDbContext(builder.Options, entityTypeManager, currentApp);
    }
}
