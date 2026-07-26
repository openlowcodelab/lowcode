using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Notification.EntityFrameworkCore;

[ConnectionStringName("NotificationDb")]
public class NotificationDbContext : AbpDbContext<NotificationDbContext>
{
    public DbSet<NotificationCategory> NotificationCategories { get; set; } = null!;
    public DbSet<NotificationChannelEntity> NotificationChannels { get; set; } = null!;
    public DbSet<ContactEntity> Contacts { get; set; } = null!;
    public DbSet<ContactGroupEntity> ContactGroups { get; set; } = null!;
    public DbSet<ContactGroupMemberEntity> ContactGroupMembers { get; set; } = null!;
    public DbSet<NotificationBusinessEntity> NotificationBusinesses { get; set; } = null!;
    public DbSet<NotificationSpecEntity> NotificationSpecs { get; set; } = null!;
    public DbSet<NotificationTemplateEntity> NotificationTemplates { get; set; } = null!;
    public DbSet<NotificationBusinessGroupEntity> NotificationBusinessGroups { get; set; } = null!;
    public DbSet<NotificationRecordEntity> NotificationRecords { get; set; } = null!;
    public DbSet<InAppRecordEntity> InAppRecords { get; set; } = null!;
    public DbSet<EmailRecordEntity> EmailRecords { get; set; } = null!;
    public DbSet<SmsRecordEntity> SmsRecords { get; set; } = null!;
    public DbSet<WebhookRecordEntity> WebhookRecords { get; set; } = null!;

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NotificationCategory>(b =>
        {
            b.ToTable("NotificationCategories");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(1000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(500);
            b.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<NotificationChannelEntity>(b =>
        {
            b.ToTable("NotificationChannels");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Code).IsRequired().HasMaxLength(64);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.ConfigJson).HasMaxLength(4000);
            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<ContactEntity>(b =>
        {
            b.ToTable("NotificationContacts");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.InAppUserId).HasMaxLength(128);
            b.Property(x => x.Email).HasMaxLength(256);
            b.Property(x => x.Phone).HasMaxLength(32);
            b.Property(x => x.WebhookUrl).HasMaxLength(500);
            b.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<ContactGroupEntity>(b =>
        {
            b.ToTable("NotificationContactGroups");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(500);
            b.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<ContactGroupMemberEntity>(b =>
        {
            b.ToTable("NotificationContactGroupMembers");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.GroupId, x.ContactId }).IsUnique();

            b.HasOne(x => x.Contact)
                .WithMany(x => x.Memberships)
                .HasForeignKey(x => x.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Group)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationBusinessEntity>(b =>
        {
            b.ToTable("NotificationBusinesses");
            b.HasKey(x => x.Id);
            b.Property(x => x.BusinessName).IsRequired().HasMaxLength(128);
            b.Property(x => x.BusinessCode).IsRequired().HasMaxLength(80);
            b.Property(x => x.Description).HasMaxLength(500);

            b.HasIndex(x => x.BusinessCode).IsUnique();
            b.HasIndex(x => x.BusinessName);
            b.HasIndex(x => x.CategoryId);
        });

        modelBuilder.Entity<NotificationSpecEntity>(b =>
        {
            b.ToTable("NotificationSpecs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Channels).HasMaxLength(64);
            b.Property(x => x.Threshold).HasColumnType("decimal(18,4)");
            b.HasIndex(x => new { x.BusinessId, x.Level }).IsUnique();

            b.HasOne(x => x.Business)
                .WithMany(x => x.Specs)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationTemplateEntity>(b =>
        {
            b.ToTable("NotificationTemplates");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).HasMaxLength(500);
            b.Property(x => x.Content).HasMaxLength(4000);
            b.HasIndex(x => new { x.BusinessId, x.ChannelType }).IsUnique();

            b.HasOne(x => x.Business)
                .WithMany(x => x.Templates)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationBusinessGroupEntity>(b =>
        {
            b.ToTable("NotificationBusinessGroups");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.BusinessId, x.GroupId }).IsUnique();

            b.HasOne(x => x.Business)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.BusinessId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationRecordEntity>(b =>
        {
            b.ToTable("NotificationRecords");
            b.HasKey(x => x.Id);
            b.Property(x => x.BusinessName).HasMaxLength(128);
            b.Property(x => x.BusinessCode).HasMaxLength(80);
            b.Property(x => x.Title).HasMaxLength(500);
            b.Property(x => x.Content).HasMaxLength(4000);
            b.Property(x => x.DataJson).HasMaxLength(4000);
            b.Property(x => x.TriggerSource).HasMaxLength(256);
            b.HasIndex(x => x.BusinessId);
            b.HasIndex(x => x.CreationTime);
        });

        ConfigureChannelRecord<InAppRecordEntity>(modelBuilder, "NotificationInAppRecords", b =>
        {
            b.Property(x => x.TargetUserId).HasMaxLength(128);
            b.HasIndex(x => new { x.TargetUserId, x.IsRead });
            b.HasOne(x => x.Record).WithMany(r => r.InAppRecords)
                .HasForeignKey(x => x.RecordId).OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureChannelRecord<EmailRecordEntity>(modelBuilder, "NotificationEmailRecords", b =>
        {
            b.Property(x => x.ToAddress).HasMaxLength(256);
            b.HasOne(x => x.Record).WithMany(r => r.EmailRecords)
                .HasForeignKey(x => x.RecordId).OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureChannelRecord<SmsRecordEntity>(modelBuilder, "NotificationSmsRecords", b =>
        {
            b.Property(x => x.Phone).HasMaxLength(32);
            b.HasOne(x => x.Record).WithMany(r => r.SmsRecords)
                .HasForeignKey(x => x.RecordId).OnDelete(DeleteBehavior.Cascade);
        });

        ConfigureChannelRecord<WebhookRecordEntity>(modelBuilder, "NotificationWebhookRecords", b =>
        {
            b.Property(x => x.Url).HasMaxLength(500);
            b.HasOne(x => x.Record).WithMany(r => r.WebhookRecords)
                .HasForeignKey(x => x.RecordId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureChannelRecord<TEntity>(
        ModelBuilder modelBuilder,
        string tableName,
        Action<Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TEntity>> extra)
        where TEntity : class
    {
        modelBuilder.Entity<TEntity>(b =>
        {
            b.ToTable(tableName);
            b.Property("RecordId");
            b.Property("BusinessName").HasMaxLength(128);
            b.Property("ContactName").HasMaxLength(128);
            b.Property("Title").HasMaxLength(500);
            b.Property("Content").HasMaxLength(4000);
            b.Property("ErrorMessage").HasMaxLength(2000);
            b.HasIndex("RecordId");
            b.HasIndex("Status");
            extra(b);
        });
    }
}
