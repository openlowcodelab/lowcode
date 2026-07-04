using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Notification.EntityFrameworkCore;

[ConnectionStringName("NotificationDb")]
public class NotificationDbContext : AbpDbContext<NotificationDbContext>
{
    /// <summary>
    /// 通知业务表
    /// </summary>
    public DbSet<NotificationBusinessEntity> NotificationBusinesses { get; set; } = null!;

    /// <summary>
    /// 通知方式配置表
    /// </summary>
    public DbSet<NotificationMethodConfigEntity> NotificationMethodConfigs { get; set; } = null!;

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NotificationBusinessEntity>(b =>
        {
            b.ToTable("NotificationBusinesses");
            b.HasKey(x => x.Id);
            b.Property(x => x.BusinessName).IsRequired().HasMaxLength(128);
            b.Property(x => x.BusinessCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(500);

            b.HasIndex(x => x.BusinessCode).IsUnique();
            b.HasIndex(x => x.BusinessName);
        });

        modelBuilder.Entity<NotificationMethodConfigEntity>(b =>
        {
            b.ToTable("NotificationMethodConfigs");
            b.HasKey(x => x.Id);
            b.Property(x => x.ConfigValue).HasMaxLength(2000);
            b.Property(x => x.WebhookUrl).HasMaxLength(500);
            b.Property(x => x.SmsTemplateId).HasMaxLength(128);
            b.Property(x => x.EmailAddress).HasMaxLength(256);

            b.HasIndex(x => x.BusinessId);

            b.HasOne(x => x.Business)
                .WithMany(x => x.Methods)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
