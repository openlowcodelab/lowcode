using H.Organization.Domain;
using Microsoft.EntityFrameworkCore;

namespace H.Organization.EntityFrameworkCore;

/// <summary>
/// 组织架构数据库上下文
/// </summary>
public class OrganizationDbContext : DbContext
{
    public OrganizationDbContext(DbContextOptions<OrganizationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=OrganizationDb;Trusted_Connection=true;",
                b => b.MigrationsAssembly("H.Organization.DbMigrator")
            );
        }
    }

    /// <summary>
    /// 部门/组织
    /// </summary>
    public DbSet<OrganizationEntity> Organizations { get; set; } = null!;

    /// <summary>
    /// 成员
    /// </summary>
    public DbSet<MemberEntity> Members { get; set; } = null!;

    /// <summary>
    /// 角色
    /// </summary>
    public DbSet<RoleEntity> Roles { get; set; } = null!;

    /// <summary>
    /// 角色成员关联
    /// </summary>
    public DbSet<RoleMember> RoleMembers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Organization 配置
        modelBuilder.Entity<OrganizationEntity>(entity =>
        {
            entity.ToTable("Organization_Organizations");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Remark).HasMaxLength(500);

            // 自引用关系配置（父子层级）
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Member 配置
        modelBuilder.Entity<MemberEntity>(entity =>
        {
            entity.ToTable("Organization_Members");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Remark).HasMaxLength(500);

            entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.Organization)
                .WithMany(e => e.Members)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Role 配置
        modelBuilder.Entity<RoleEntity>(entity =>
        {
            entity.ToTable("Organization_Roles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Remark).HasMaxLength(500);

            entity.HasIndex(e => e.Code).IsUnique();
        });

        // RoleMember 配置
        modelBuilder.Entity<RoleMember>(entity =>
        {
            entity.ToTable("Organization_RoleMembers");
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => new { e.RoleId, e.MemberId }).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(e => e.RoleMembers)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Member)
                .WithMany()
                .HasForeignKey(e => e.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
