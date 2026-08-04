using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.File.EntityFrameworkCore;

[ConnectionStringName("FileDb")]
public class FileDbContext : AbpDbContext<FileDbContext>
{
    public DbSet<FileProjectEntity> FileProjects { get; set; } = null!;
    public DbSet<FileFolderEntity> FileFolders { get; set; } = null!;
    public DbSet<FileObjectEntity> FileObjects { get; set; } = null!;

    public FileDbContext(DbContextOptions<FileDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FileProjectEntity>(b =>
        {
            b.ToTable("FileProjects");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(50);
            b.Property(x => x.Code).IsRequired().IsUnicode(false).HasMaxLength(20);
            b.Property(x => x.Description).HasMaxLength(50);
            b.Property(x => x.Icon).HasMaxLength(64);
            b.Property(x => x.BucketName).IsRequired().IsUnicode(false).HasMaxLength(20);
            b.HasIndex(x => x.BucketName).IsUnique();
            b.Property(x => x.FileCount).HasDefaultValue(0);
            b.Property(x => x.TotalSize).HasDefaultValue(0);
        });

        modelBuilder.Entity<FileFolderEntity>(b =>
        {
            b.ToTable("FileFolders");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProjectId).IsRequired();
            b.Property(x => x.Code).IsRequired().IsUnicode(false).HasMaxLength(20);
            b.Property(x => x.Name).IsRequired().HasMaxLength(50);
            b.Property(x => x.Path).IsRequired().IsUnicode(false).HasMaxLength(20);

            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<FileObjectEntity>(b =>
        {
            b.ToTable("FileObjects");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProjectId).IsRequired();
            b.Property(x => x.FileName).IsRequired().HasMaxLength(50);
            b.Property(x => x.Size).IsRequired();
            b.Property(x => x.ContentType).IsRequired().IsUnicode(false).HasMaxLength(50);
            b.Property(x => x.FolderPath).IsRequired().IsUnicode(false).HasMaxLength(130);

            b.HasIndex(x => new { x.ProjectId, x.FolderPath });
        });
    }
}
