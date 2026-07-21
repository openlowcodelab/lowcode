using H.Testing.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace H.Testing.DbMigrator;

/// <summary>
/// 仅用于数据种子的纯 DbContext（映射与 TestingDbContext 相同的表）。
/// 不继承 AbpDbContext，从而避开 ABP 软删除/多租户查询过滤器与审计拦截，
/// 由种子逻辑手动写入 TenantId 与审计列。
/// </summary>
public class TestingSeedDbContext : DbContext
{
    public DbSet<TestingProject> Projects => Set<TestingProject>();
    public DbSet<TestingProjectService> ProjectServices => Set<TestingProjectService>();
    public DbSet<TestingProjectEnvironment> ProjectEnvironments => Set<TestingProjectEnvironment>();
    public DbSet<TestingEnvironmentServiceConfig> EnvironmentServiceConfigs => Set<TestingEnvironmentServiceConfig>();
    public DbSet<TestingProjectCaseCategory> ProjectCaseCategories => Set<TestingProjectCaseCategory>();
    public DbSet<TestingProjectCase> ProjectCases => Set<TestingProjectCase>();
    public DbSet<TestingExecutionRecord> ExecutionRecords => Set<TestingExecutionRecord>();

    public TestingSeedDbContext(DbContextOptions<TestingSeedDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestingProject>().ToTable("TestingProjects");
        modelBuilder.Entity<TestingProjectService>().ToTable("TestingProjectServices");
        modelBuilder.Entity<TestingProjectEnvironment>().ToTable("TestingProjectEnvironments");
        modelBuilder.Entity<TestingEnvironmentServiceConfig>().ToTable("TestingEnvironmentServiceConfigs");
        modelBuilder.Entity<TestingProjectCaseCategory>().ToTable("TestingProjectCaseCategories");
        modelBuilder.Entity<TestingProjectCase>().ToTable("TestingProjectCases");
        modelBuilder.Entity<TestingExecutionRecord>().ToTable("TestingExecutionRecords");
    }
}
