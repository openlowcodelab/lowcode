using H.Organization.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace H.Organization.DbMigrator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("  H.Organization.DbMigrator - 数据库迁移工具");
        Console.WriteLine("==========================================");
        Console.WriteLine();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("OrganizationDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=OrganizationDb;Trusted_Connection=true;";

        Console.WriteLine($"连接字符串：{connectionString}");
        Console.WriteLine();

        var optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var dbContext = new OrganizationDbContext(optionsBuilder.Options);

        // 应用所有迁移
        Console.WriteLine("正在应用迁移...");
        
        // 检查待应用的迁移
        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            Console.WriteLine($"发现 {pendingMigrations.Count()} 个待应用的迁移:");
            foreach (var migration in pendingMigrations)
            {
                Console.WriteLine($"  - {migration}");
            }
            Console.WriteLine();
            
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("迁移应用成功！");
        }
        else
        {
            Console.WriteLine("没有待应用的迁移，数据库已是最新状态。");
        }

        Console.WriteLine();
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
