using H.LowCode.Application.Contracts;
using H.LowCode.DbMigrator.Services;
using H.LowCode.DesignEngine.EntityFrameworkCore;
using H.LowCode.Entity;
using Microsoft.EntityFrameworkCore;

namespace H.LowCode.DbMigrator;

/// <summary>
/// 用于迁移数据库
/// 说明: 直接使用 DesignEngineDbContext 会在 DesignEngineDbContext 所在项目(即"H.LowCode.EntityFrameworkCore")中找 Migrations 文件夹下的迁移文件
///       由于迁移文件是在"H.LowCode.DbMigrator"项目中生成的, 所以在 "H.LowCode.DbMigrator" 中重新定义一个 DbContext
/// </summary>
public class MigratorDbContext : DesignEngineDbContext
{
    private readonly EntityTypeManager _entityTypeManager;
    private readonly MigrationAppContextService _appContextService;

    public MigratorDbContext(DbContextOptions<DesignEngineDbContext> options,
        EntityTypeManager entityTypeManager,
        MigrationAppContextService appContextService) : base(options, entityTypeManager, appContextService)
    {
        _entityTypeManager = entityTypeManager;
        _appContextService = appContextService;
    }

    protected override IList<DynamicEntityInfo> GetEntityTypes()
    {
        IList<DynamicEntityInfo> dynamicEntities = [];
        // 同步等待遍历所有应用，确保实体完整加载
        _appContextService.ForEachAppAsync(async (appId) =>
        {
            var currentAppDynamicEntities = _entityTypeManager.LoadDynamicEntities(appId);
            dynamicEntities = [.. dynamicEntities, .. currentAppDynamicEntities];
        }).GetAwaiter().GetResult();
        return dynamicEntities;
    }
}
