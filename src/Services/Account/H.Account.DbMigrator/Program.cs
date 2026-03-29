using H.Account.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace H.Account.DbMigrator;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("==========================================");
        Console.WriteLine("  H.Account.DbMigrator - 数据库迁移工具");
        Console.WriteLine("==========================================");
        Console.WriteLine();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("AccountDb")
            ?? "Server=(localdb)\\mssqllocaldb;Database=AccountDb;Trusted_Connection=true;";

        Console.WriteLine($"连接字符串：{connectionString}");
        Console.WriteLine();

        var optionsBuilder = new DbContextOptionsBuilder<AccountDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var dbContext = new AccountDbContext(optionsBuilder.Options);

        // 应用所有迁移
        Console.WriteLine("正在应用迁移...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("迁移应用成功！");

        Console.WriteLine();
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}
