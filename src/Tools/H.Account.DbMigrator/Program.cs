using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using H.Account.EntityFrameworkCore;
using Volo.Abp.Identity;

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
                var dbContext = services.GetRequiredService<AccountDbContext>();

                Console.WriteLine("开始执行数据库迁移...");

                // 执行数据库迁移
                await dbContext.Database.MigrateAsync();

                Console.WriteLine("数据库迁移完成");

                // 种子数据：系统内置角色
                await SeedSystemRolesAsync(dbContext);
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

    /// <summary>
    /// 系统内置角色种子数据
    /// </summary>
    private static async Task SeedSystemRolesAsync(AccountDbContext dbContext)
    {
        var builtInRoles = new[] { "SuperAdmin", "Admin" };

        foreach (var roleName in builtInRoles)
        {
            var existing = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (existing == null)
            {
                var role = new IdentityRole(Guid.NewGuid(), roleName);
                role.IsStatic = true;
                dbContext.Roles.Add(role);
                Console.WriteLine($"已创建内置角色: {roleName}");
            }
            else if (!existing.IsStatic)
            {
                existing.IsStatic = true;
                Console.WriteLine($"已更新内置角色 IsStatic: {roleName}");
            }
        }

        await dbContext.SaveChangesAsync();
        Console.WriteLine("角色种子数据完成");
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
                services.AddDbContext<AccountDbContext>(options =>
                    options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Namespace)));
            });
}
