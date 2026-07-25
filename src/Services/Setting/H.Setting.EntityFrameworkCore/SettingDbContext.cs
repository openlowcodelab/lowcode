using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Setting.EntityFrameworkCore;

/// <summary>
/// 配置管理数据库上下文
/// </summary>
[ConnectionStringName("SettingDb")]
public class SettingDbContext : AbpDbContext<SettingDbContext>
{
    /// <summary>配置定义表</summary>
    public DbSet<SettingDefinition> SettingDefinitions { get; set; } = null!;

    /// <summary>配置项（配置值）表</summary>
    public DbSet<SettingValue> SettingValues { get; set; } = null!;

    public SettingDbContext(DbContextOptions<SettingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SettingDefinition>(b =>
        {
            b.ToTable("AppSettingDefinitions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.DefaultValue).HasColumnType("nvarchar(max)");
            b.Property(x => x.Providers).HasMaxLength(1024);
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<SettingValue>(b =>
        {
            b.ToTable("AppSettingValues");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Value).HasColumnType("nvarchar(max)");
            b.Property(x => x.ProviderName).IsRequired().HasMaxLength(64);
            b.Property(x => x.ProviderKey).HasMaxLength(64);
            b.HasIndex(x => new { x.Name, x.ProviderName, x.ProviderKey }).IsUnique();
        });
    }
}
