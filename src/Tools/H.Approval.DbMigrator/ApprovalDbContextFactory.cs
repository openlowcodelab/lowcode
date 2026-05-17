using H.Approval.EntityFrameworkCore;
using H.LowCode.DbMigrator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace H.Approval.DbMigrator;

/// <summary>
/// 执行 dotnet ef migrations [command] [arguments] 命令时使用
/// </summary>
public class ApprovalDbContextFactory : IDesignTimeDbContextFactory<MigratorDbContext>
{
    public MigratorDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        //指定迁移文件生成到当前程序集（DbMigrator）中，而不是主项目中
        string connectionString = configuration.GetConnectionString("ApprovalDb")!;
        var optionsBuilder = new DbContextOptionsBuilder<ApprovalDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        return new MigratorDbContext(optionsBuilder.Options);
    }
}
