using H.Order.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace H.Order.DbMigrator;

/// <summary>
/// 执行 dotnet ef migrations [command] [arguments] 命令时使用
/// </summary>
public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        string connectionString = configuration.GetConnectionString("OrderDb")!;
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        return new OrderDbContext(optionsBuilder.Options);
    }
}