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
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Description).HasMaxLength(512);
            b.Property(x => x.Icon).HasMaxLength(64);
            b.Property(x => x.BucketName).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.BucketName).IsUnique();
            b.Property(x => x.FileCount).HasDefaultValue(0);
            b.Property(x => x.TotalSize).HasDefaultValue(0);
        });

        modelBuilder.Entity<FileFolderEntity>(b =>
        {
            b.ToTable("FileFolders");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProjectId).IsRequired();
            b.Property(x => x.Code).IsRequired().HasMaxLength(20);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Path).IsRequired().HasMaxLength(512);
            b.HasIndex(x => new { x.ProjectId, x.Path }).IsUnique();
        });

        modelBuilder.Entity<FileObjectEntity>(b =>
        {
            b.ToTable("FileObjects");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProjectId).IsRequired();
            b.Property(x => x.Key).IsRequired().HasMaxLength(1024);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Size).IsRequired();
            b.Property(x => x.ContentType).IsRequired().HasMaxLength(128);
            b.Property(x => x.FolderPath).IsRequired().HasMaxLength(512);
            b.HasIndex(x => new { x.ProjectId, x.Key }).IsUnique();
            b.HasIndex(x => new { x.ProjectId, x.FolderPath });
        });
    }
}
