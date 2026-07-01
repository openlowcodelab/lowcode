using H.Enterprise.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace H.Enterprise.DbMigrator;

/// <summary>
/// 执行 dotnet ef migrations 命令时使用
/// </summary>
/// <remarks>新增迁移文件: dotnet ef migrations add xxx</remarks>
public class EnterpriseDbContextFactory : IDesignTimeDbContextFactory<EnterpriseDbContext>
{
    public EnterpriseDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        string connectionString = configuration.GetConnectionString("EnterpriseDb")!;
        var optionsBuilder = new DbContextOptionsBuilder<EnterpriseDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        return new EnterpriseDbContext(optionsBuilder.Options);
    }
}
