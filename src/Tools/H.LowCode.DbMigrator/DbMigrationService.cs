using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.DbMigrator;

public class DbMigrationService : ITransientDependency
{
    public ILogger<DbMigrationService> Logger { get; set; }

    private readonly IDataSeeder _dataSeeder;
    private readonly IEnumerable<IDbSchemaMigrator> _dbSchemaMigrators;
    private readonly MigrationCurrentApp _currentApp;

    public DbMigrationService(
        IDataSeeder dataSeeder,
        IEnumerable<IDbSchemaMigrator> dbSchemaMigrators,
        MigrationCurrentApp currentApp)
    {
        _dataSeeder = dataSeeder;
        _dbSchemaMigrators = dbSchemaMigrators;
        _currentApp = currentApp;

        Logger = NullLogger<DbMigrationService>.Instance;
    }

    public async Task MigrateAsync()
    {
        Logger.LogInformation("Started database migrations...");

        // 首先执行数据库架构迁移（不依赖 AppId）
        await MigrateDatabaseSchemaAsync();

        // 然后遍历所有应用进行数据种子
        await _currentApp.ForEachAppAsync(async (appId) =>
        {
            Logger.LogInformation("开始为应用 {AppId} 执行数据种子", appId);
            await SeedDataAsync();
        });

        Logger.LogInformation("Successfully completed all database migrations.");
        Logger.LogInformation("You can safely end this process...");
    }

    private async Task MigrateDatabaseSchemaAsync()
    {
        foreach (var migrator in _dbSchemaMigrators)
        {
            await migrator.MigrateAsync();
        }
    }

    private async Task SeedDataAsync()
    {
        await _dataSeeder.SeedAsync();
    }
}
