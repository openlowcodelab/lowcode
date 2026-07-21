using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 测试模块数据库上下文
/// </summary>
[ConnectionStringName("TestingDb")]
public class TestingDbContext : AbpDbContext<TestingDbContext>
{
    public DbSet<TestingProject> Projects { get; set; } = null!;
    public DbSet<TestingProjectService> ProjectServices { get; set; } = null!;
    public DbSet<TestingProjectEnvironment> ProjectEnvironments { get; set; } = null!;
    public DbSet<TestingEnvironmentServiceConfig> EnvironmentServiceConfigs { get; set; } = null!;
    public DbSet<TestingProjectCaseCategory> ProjectCaseCategories { get; set; } = null!;
    public DbSet<TestingProjectCase> ProjectCases { get; set; } = null!;
    public DbSet<TestingExecutionRecord> ExecutionRecords { get; set; } = null!;

    public TestingDbContext(DbContextOptions<TestingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestingProject>(b =>
        {
            b.ToTable("TestingProjects");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.EnvironmentIdsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(128);
            b.Property(x => x.UpdatedBy).HasMaxLength(128);
            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<TestingProjectService>(b =>
        {
            b.ToTable("TestingProjectServices");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.CreatedBy).HasMaxLength(128);
            b.Property(x => x.UpdatedBy).HasMaxLength(128);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<TestingProjectEnvironment>(b =>
        {
            b.ToTable("TestingProjectEnvironments");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.VariablesJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.HeadersJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.DatabaseConfigJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(128);
            b.Property(x => x.UpdatedBy).HasMaxLength(128);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<TestingEnvironmentServiceConfig>(b =>
        {
            b.ToTable("TestingEnvironmentServiceConfigs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.BaseUrl).HasMaxLength(500);
            b.Property(x => x.CreatedBy).HasMaxLength(128);
            b.HasIndex(x => x.EnvironmentId);
            b.HasIndex(x => x.ProjectServiceId);
            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<TestingProjectCaseCategory>(b =>
        {
            b.ToTable("TestingProjectCaseCategories");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.CreatedBy).HasMaxLength(128);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.ParentId);
            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<TestingProjectCase>(b =>
        {
            b.ToTable("TestingProjectCases");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.CaseNumber).HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.LevelsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.TagsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.StepsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.TestDataJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.CreatedBy).HasMaxLength(128);
            b.Property(x => x.UpdatedBy).HasMaxLength(128);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.CategoryId);
            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<TestingExecutionRecord>(b =>
        {
            b.ToTable("TestingExecutionRecords");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.TestCaseName).HasMaxLength(200);
            b.Property(x => x.EnvironmentName).HasMaxLength(100);
            b.Property(x => x.StepRecordsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.EnvironmentSnapshotJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.ErrorMessage).HasColumnType("nvarchar(max)");
            b.Property(x => x.ExecutedBy).HasMaxLength(128);
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.TestCaseId);
            b.HasIndex(x => x.EnvironmentId);
            b.HasIndex(x => x.TenantId);
        });
    }
}
