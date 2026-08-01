using H.File.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace H.File.DbMigrator;

/// <summary>
/// 执行 dotnet ef migrations [command] [arguments] 命令时使用
/// </summary>
public class FileDbContextFactory : IDesignTimeDbContextFactory<FileDbContext>
{
    public FileDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        string connectionString = configuration.GetConnectionString("FileDb")!;
        var optionsBuilder = new DbContextOptionsBuilder<FileDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        return new FileDbContext(optionsBuilder.Options);
    }
}
