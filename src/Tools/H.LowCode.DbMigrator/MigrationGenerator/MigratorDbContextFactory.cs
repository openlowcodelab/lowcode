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

        //此处使用的是 LowCodeDbContext, 默认生成的迁移文件会在 LowCodeDbContext 所在的程序集(H.LowCode.EntityFrameworkCore)中
        //需通过 MigrationsAssembly 指定迁移文件生成到 "H.LowCode.DbMigrator" 程序集中
        string migrationAssembly = typeof(Program).Namespace;
        var builder = new DbContextOptionsBuilder<LowCodeDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"), b => b.MigrationsAssembly(migrationAssembly));

        var services = new ServiceCollection();
        services.AddApplication<LowCodeDbMigratorModule>();
        services.AddApplication<MetaJsonFileRepositoryModule>();
        var serviceProvider = services.BuildServiceProvider();
        EntityTypeManager entityTypeManager = serviceProvider.GetService<EntityTypeManager>();

        return new MigratorDbContext(builder.Options, entityTypeManager);
    }
}
