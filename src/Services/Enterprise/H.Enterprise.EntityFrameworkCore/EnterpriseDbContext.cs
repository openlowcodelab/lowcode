using H.Enterprise.EntityFrameworkCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace H.Enterprise.EntityFrameworkCore;

/// <summary>
/// 企业管理数据库上下文
/// 注意: Enterprise 和 EnterpriseUser 是跨租户实体，不应用租户过滤
/// </summary>
public class EnterpriseDbContext : DbContext
{
    public EnterpriseDbContext(DbContextOptions<EnterpriseDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=EnterpriseDb;Trusted_Connection=true;",
                b => b.MigrationsAssembly("H.Enterprise.DbMigrator")
            );
        }
    }

    /// <summary>
    /// 企业
    /// </summary>
    public DbSet<EnterpriseEntity> Enterprises { get; set; } = null!;

    /// <summary>
    /// 企业用户关联
    /// </summary>
    public DbSet<EnterpriseUserEntity> EnterpriseUsers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enterprise 配置
        modelBuilder.Entity<EnterpriseEntity>(entity =>
        {
            entity.ToTable("Enterprise_Enterprises");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Logo).HasMaxLength(500);
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.ContactEmail).HasMaxLength(100);
            entity.Property(e => e.ConnectionString).HasMaxLength(1000);
            entity.Property(e => e.Remark).HasMaxLength(500);

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // EnterpriseUser 配置
        modelBuilder.Entity<EnterpriseUserEntity>(entity =>
        {
            entity.ToTable("Enterprise_EnterpriseUsers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

            // 复合唯一索引: 同一企业下同一用户只能有一条关联记录
            entity.HasIndex(e => new { e.EnterpriseId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Role);

            // 外键关系
            entity.HasOne(e => e.Enterprise)
                .WithMany(e => e.EnterpriseUsers)
                .HasForeignKey(e => e.EnterpriseId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
