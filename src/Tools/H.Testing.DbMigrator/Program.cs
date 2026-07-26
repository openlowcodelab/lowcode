using H.Testing.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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
        var enterpriseConnectionString = configuration.GetConnectionString("EnterpriseDb");
        var tenantName = configuration["SeedTenantName"] ?? "HTech";

        var optionsBuilder = new DbContextOptionsBuilder<TestingDbContext>()
            .UseSqlServer(testingConnectionString, b => b.MigrationsAssembly(typeof(Program).Namespace));

        using var dbContext = new TestingDbContext(optionsBuilder.Options);

        try
        {
            Console.WriteLine("开始执行数据库迁移...");
            await dbContext.Database.MigrateAsync();
            Console.WriteLine("数据库迁移完成");

            var tenantId = await ResolveTenantIdAsync(enterpriseConnectionString!, tenantName);
            if (tenantId == null)
            {
                Console.WriteLine($"未在 Enterprise 库中找到已启用的租户 '{tenantName}'，请先创建该企业后再运行种子。已完成建表。");
            }
            else
            {
                Console.WriteLine($"解析到租户 '{tenantName}' => {tenantId}");
                var seedDir = Path.Combine(AppContext.BaseDirectory, "SeedData");

                var seedOptions = new DbContextOptionsBuilder<TestingSeedDbContext>()
                    .UseSqlServer(testingConnectionString)
                    .Options;
                using var seedContext = new TestingSeedDbContext(seedOptions);
                var seeder = new TestingDataSeeder(seedContext, tenantId.Value, seedDir);
                await seeder.SeedAsync();
            }
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

    /// <summary>
    /// 从 Enterprise 库按名称解析租户（企业）ID
    /// </summary>
    private static async Task<Guid?> ResolveTenantIdAsync(string enterpriseConnectionString, string tenantName)
    {
        const string sql = "SELECT TOP 1 Id FROM Enterprise_Enterprises WHERE Name = @name ORDER BY IsActivated DESC";
        await using var conn = new SqlConnection(enterpriseConnectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", tenantName);
        var result = await cmd.ExecuteScalarAsync();
        if (result is Guid g) return g;
        if (result != null && Guid.TryParse(result.ToString(), out var parsed)) return parsed;
        return null;
    }
}
