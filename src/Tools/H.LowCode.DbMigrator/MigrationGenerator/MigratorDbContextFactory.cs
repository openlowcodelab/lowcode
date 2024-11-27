using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H.LowCode.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using H.LowCode.Repository.JsonFile;

namespace H.LowCode.DbMigrator;

/// <summary>
/// 用于在命令行工具中生成迁移脚本: dotnet ef migrations add Initial --context DbMigratorDbContext
/// </summary>
public class MigratorDbContextFactory : IDesignTimeDbContextFactory<DbMigratorDbContext>
{
    public DbMigratorDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        string migrationAssembly = typeof(Program).Namespace;
        var builder = new DbContextOptionsBuilder<LowCodeDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"),
            b => b.MigrationsAssembly(migrationAssembly));

        var services = new ServiceCollection();
        services.AddApplication<LowCodeDbMigratorModule>();
        services.AddApplication<MetaJsonFileRepositoryModule>();
        var serviceProvider = services.BuildServiceProvider();
        EntityTypeManager entityTypeManager = serviceProvider.GetService<EntityTypeManager>();

        return new DbMigratorDbContext(builder.Options, entityTypeManager);
    }
}
