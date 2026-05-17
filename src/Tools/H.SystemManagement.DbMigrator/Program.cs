using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace H.SystemManagement.DbMigrator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            try
            {
                var dbContext = services.GetRequiredService<MigratorDbContext>();

                Console.WriteLine("开始执行数据库迁移...");

                // 执行数据库迁移
                await dbContext.Database.MigrateAsync();

                Console.WriteLine("数据库迁移完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库迁移失败: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((hostContext, services) =>
            {
                var configuration = hostContext.Configuration;
                var connectionString = configuration.GetConnectionString("SystemManagementDb");

                services.AddDbContext<MigratorDbContext>(options =>
                    options.UseSqlServer(connectionString));
            });
}
