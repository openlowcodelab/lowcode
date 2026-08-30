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
    public DbSet<ProjectEntity> Projects { get; set; } = null!;
    public DbSet<ProjectServiceEntity> ProjectServices { get; set; } = null!;
    public DbSet<ProjectEnvEntity> ProjectEnvironments { get; set; } = null!;
    public DbSet<CaseCategoryEntity> ProjectCaseCategories { get; set; } = null!;
    public DbSet<CaseEntity> ProjectCases { get; set; } = null!;
    public DbSet<CaseStepEntity> CaseSteps { get; set; } = null!;
    public DbSet<CaseRecordEntity> ExecutionRecords { get; set; } = null!;
    public DbSet<SettingsEntity> Settings { get; set; } = null!;

    public TestingDbContext(DbContextOptions<TestingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProjectEntity>(b =>
        {
            b.ToTable("Project");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(100000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(20);
            b.Property(x => x.Status);
            b.Property(x => x.KnowledgeBaseId).HasMaxLength(36);
            b.Property(x => x.Description).HasMaxLength(100);

            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<ProjectServiceEntity>(b =>
        {
            b.ToTable("ProjectService");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(2000000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(20);
            b.Property(x => x.ProjectId).IsRequired();
            b.Property(x => x.Description).HasMaxLength(100);

            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<ProjectEnvEntity>(b =>
        {
            b.ToTable("ProjectEnv");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(3000000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(20);
            b.Property(x => x.Description).HasMaxLength(100);
            b.Property(x => x.VariablesJson).HasMaxLength(2000);
            b.Property(x => x.HeadersJson).HasMaxLength(1000);
            b.Property(x => x.ServiceConfigsJson).HasMaxLength(1000);

            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<CaseCategoryEntity>(b =>
        {
            b.ToTable("CaseCategory");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(7000000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(20);

            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<CaseEntity>(b =>
        {
            b.ToTable("Case");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(8000000, 1);
            b.Property(x => x.CaseName).IsRequired().HasMaxLength(50);
            b.Property(x => x.Description).HasMaxLength(200);
            b.Property(x => x.Level);

            b.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<CaseStepEntity>(b =>
        {
            b.ToTable("CaseStep");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(8500000, 1);
            b.Property(x => x.Name).IsRequired().HasMaxLength(50);
            b.Property(x => x.Order);
            b.Property(x => x.Type);
            b.Property(x => x.IsEnabled);
            b.Property(x => x.ParametersJson).HasMaxLength(4000);
            b.Property(x => x.ApiConfigJson).HasMaxLength(4000);
            b.Property(x => x.UiConfigJson).HasMaxLength(4000);
            b.Property(x => x.ScriptConfigJson).HasMaxLength(4000);
            b.Property(x => x.ExpectedResult).HasMaxLength(4000);

            b.HasIndex(x => x.CaseId);
        });

        modelBuilder.Entity<CaseRecordEntity>(b =>
        {
            b.ToTable("CaseExecutionRecord");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(9000000, 1);
            b.Property(x => x.CaseName).HasMaxLength(50);
            b.Property(x => x.EnvName).HasMaxLength(20);
            // 步骤执行详情（含执行参数、断言结果、日志等）内容较大，使用 nvarchar(max) 避免截断
            b.Property(x => x.StepRecordsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.EnvSnapshotJson).HasMaxLength(4000);
            b.Property(x => x.ErrorMessage).HasMaxLength(1000);
            b.Property(x => x.ExecutedBy).HasMaxLength(36).IsUnicode(false);

            b.HasIndex(x => x.CaseId);
        });

        modelBuilder.Entity<SettingsEntity>(b =>
        {
            b.ToTable("Settings");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(1000, 1);
            b.Property(x => x.Key).HasMaxLength(50).IsRequired().IsUnicode(false);
            b.Property(x => x.Value).HasMaxLength(500);
        });
    }
}
