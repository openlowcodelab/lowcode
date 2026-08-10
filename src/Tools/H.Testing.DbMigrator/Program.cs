using H.Testing.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace H.Testing.DbMigrator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var testingConnectionString = configuration.GetConnectionString("TestingDb");

        var optionsBuilder = new DbContextOptionsBuilder<TestingDbContext>()
            .UseSqlServer(testingConnectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        using var dbContext = new TestingDbContext(optionsBuilder.Options);

        try
        {
            Console.WriteLine("开始执行数据库迁移...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("数据库迁移完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行失败: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
}
