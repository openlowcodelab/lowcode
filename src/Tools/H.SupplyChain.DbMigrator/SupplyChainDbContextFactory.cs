using H.SupplyChain.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace H.SupplyChain.DbMigrator;

/// <summary>
/// 执行 dotnet ef migrations [command] [arguments] 命令时使用
/// </summary>
public class SupplyChainDbContextFactory : IDesignTimeDbContextFactory<SupplyChainDbContext>
{
    public SupplyChainDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        string connectionString = configuration.GetConnectionString("SupplyChainDb")!;
        var optionsBuilder = new DbContextOptionsBuilder<SupplyChainDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        return new SupplyChainDbContext(optionsBuilder.Options);
    }
}