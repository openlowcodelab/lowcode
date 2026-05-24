using H.Assistant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace H.Assistant.DbMigrator;

/// <summary>
/// 执行 dotnet ef migrations 命令时使用
/// </summary>
/// <remarks>新增迁移文件: dotnet ef migrations add xxx</remarks>
public class AssistantDbContextFactory : IDesignTimeDbContextFactory<AssistantDbContext>
{
    public AssistantDbContext CreateDbContext(string[] args)
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false);
        var configuration = configurationBuilder.Build();

        //指定迁移文件生成到当前程序集（DbMigrator）中，而不是主项目中
        string connectionString = configuration.GetConnectionString("AssistantDb")!;
        var optionsBuilder = new DbContextOptionsBuilder<AssistantDbContext>()
            .UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        return new AssistantDbContext(optionsBuilder.Options);
    }
}
