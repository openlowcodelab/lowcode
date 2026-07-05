using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

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

                // 种子数据：系统内置角色
                await SeedSystemRolesAsync(dbContext);

                // 种子数据：系统管理员用户
                await SeedSystemUserAsync(dbContext);
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
    private static async Task SeedSystemRolesAsync(MigratorDbContext dbContext)
    {
        var roles = dbContext.Set<IdentityRole>();
        var builtInRoles = new[] { "SuperAdmin", "Admin" };

        foreach (var roleName in builtInRoles)
        {
            var existing = await roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (existing == null)
            {
                var role = new IdentityRole(Guid.NewGuid(), roleName);
                role.IsStatic = true;
                roles.Add(role);
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

    /// <summary>
    /// 超级管理员用户种子数据
    /// </summary>
    private static async Task SeedSystemUserAsync(MigratorDbContext dbContext)
    {
        const string userName = "sys";
        const string password = "Sys,123456";
        const string email = "sys@applab.com";

        var users = dbContext.Set<IdentityUser>();
        var roles = dbContext.Set<IdentityRole>();

        var existingUser = await users.FirstOrDefaultAsync(u => u.UserName == userName);
        if (existingUser != null)
        {
            Console.WriteLine($"超级管理员用户 '{userName}' 已存在，跳过创建");
            return;
        }

        // 使用 ASP.NET Core Identity 的 PasswordHasher 生成密码哈希
        var userId = Guid.NewGuid();
        var securityStamp = Guid.NewGuid().ToString("N");
        var concurrencyStamp = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;

        // PasswordHasher 需要 IdentityUser 实例，但只用于生成标准哈希格式
        var tempUser = new IdentityUser(userId, userName, email);
        var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<IdentityUser>();
        var hashedPassword = passwordHasher.HashPassword(tempUser, password);

        // 使用原始 SQL 插入用户记录（ABP IdentityUser 属性 setter 受保护，无法直接赋值）
        await dbContext.Database.ExecuteSqlRawAsync(
            @"INSERT INTO AbpUsers 
                (Id, UserName, NormalizedUserName, Email, NormalizedEmail, PasswordHash, SecurityStamp, ConcurrencyStamp, 
                 IsActive, IsDeleted, IsExternal, EmailConfirmed, PhoneNumberConfirmed, LockoutEnabled, 
                 AccessFailedCount, TwoFactorEnabled, ShouldChangePasswordOnNextLogin, Leaved, 
                 CreationTime, EntityVersion, ExtraProperties)
              VALUES 
                ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 
                 {8}, {9}, {10}, {11}, {12}, {13}, 
                 {14}, {15}, {16}, {17}, 
                 {18}, {19}, {20})",
            userId, userName, userName.ToUpperInvariant(), email, email.ToUpperInvariant(),
            hashedPassword, securityStamp, concurrencyStamp,
            true, false, false, true, true, false,
            0, false, false, false,
            now, 1, "{}");

        // 将用户关联到 SuperAdmin 角色（使用原始 SQL 插入关联关系）
        var superAdminRole = await roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole != null)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                "INSERT INTO AbpUserRoles (UserId, RoleId) VALUES ({0}, {1})",
                userId, superAdminRole.Id);
        }

        Console.WriteLine($"已创建超级管理员用户: {userName}，默认密码: {password}");
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
