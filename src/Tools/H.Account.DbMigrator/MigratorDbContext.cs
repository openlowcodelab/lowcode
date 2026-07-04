using Microsoft.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace H.Account.DbMigrator;

/// <summary>
/// 用于 DbMigrator 的 DbContext，显式配置 Identity 模型
/// </summary>
public class MigratorDbContext : DbContext
{
    public MigratorDbContext(DbContextOptions<MigratorDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 显式配置 Identity 实体（与 AccountDbContext 保持一致）
        modelBuilder.ConfigureIdentity();
    }
}
