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

public class MigratorDbContextFactory : IDesignTimeDbContextFactory<LowCodeDbContext>
{
    public LowCodeDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        string migrationAssembly = typeof(Program).Namespace;
        var builder = new DbContextOptionsBuilder<LowCodeDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"),
            b=> b.MigrationsAssembly(migrationAssembly));

        var services = new ServiceCollection();
        services.AddApplication<LowCodeDbMigratorModule>();
        services.AddApplication<MetaJsonFileRepositoryModule>();
        var serviceProvider = services.BuildServiceProvider();
        EntityTypeManager entityTypeManager = serviceProvider.GetService<EntityTypeManager>();

        return new LowCodeDbContext(builder.Options, entityTypeManager);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
