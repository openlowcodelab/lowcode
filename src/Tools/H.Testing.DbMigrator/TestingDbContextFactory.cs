using H.Testing.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace H.Testing.DbMigrator;

/// <summary>
/// 执行 dotnet ef migrations [command] [arguments] 命令时使用
/// </summary>
public class TestingDbContextFactory : IDesignTimeDbContextFactory<TestingDbContext>
{
    public TestingDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        string connectionString = configuration.GetConnectionString("TestingDb")!;
        var optionsBuilder = new DbContextOptionsBuilder<TestingDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        return new TestingDbContext(optionsBuilder.Options);
    }
}
