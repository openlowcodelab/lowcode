using H.BackgroundTask.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace H.BackgroundTask.DbMigrator;

/// <summary>
/// 执行 dotnet ef migrations [command] [arguments] 命令时使用
/// </summary>
public class BackgroundTaskDbContextFactory : IDesignTimeDbContextFactory<BackgroundTaskDbContext>
{
    public BackgroundTaskDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        string connectionString = configuration.GetConnectionString("BackgroundTaskDb")!;
        var optionsBuilder = new DbContextOptionsBuilder<BackgroundTaskDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        return new BackgroundTaskDbContext(optionsBuilder.Options);
    }
}
