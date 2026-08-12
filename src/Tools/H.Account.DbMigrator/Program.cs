using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace H.Account.DbMigrator;

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
                // 使用 MigratorDbContext 而非 AccountDbContext，因为后者继承自 AbpDbContext，
                // 其模型配置依赖 ABP 模块系统初始化。MigratorDbContext 是原生 DbContext，
                // 显式调用 ConfigureIdentity() 配置模型，适合在 DbMigrator 中使用。
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
                var connectionString = configuration.GetConnectionString("AccountDb");

                // MigrationsAssembly 用于指定迁移文件所在的程序集
                services.AddDbContext<MigratorDbContext>(options =>
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace)));
            });
}
