using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.File.EntityFrameworkCore;

[ConnectionStringName("FileDb")]
public class FileDbContext : AbpDbContext<FileDbContext>
{
    public DbSet<FileProjectEntity> FileProjects { get; set; } = null!;

    public FileDbContext(DbContextOptions<FileDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileProjectEntity>(b =>
        {
            b.ToTable("FileProjects");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.Icon).HasMaxLength(64);
            b.Property(x => x.BucketName).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.BucketName).IsUnique();
        });
    }
}
