using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.BackgroundTask.EntityFrameworkCore;

/// <summary>
/// 后台任务应用数据库上下文
/// </summary>
[ConnectionStringName("BackgroundTaskDb")]
public class BackgroundTaskDbContext : AbpDbContext<BackgroundTaskDbContext>
{
    /// <summary>任务定义表</summary>
    public DbSet<BackgroundJobEntity> BackgroundJobs { get; set; } = null!;

    /// <summary>执行记录表</summary>
    public DbSet<JobExecutionRecordEntity> JobExecutionRecords { get; set; } = null!;

    public BackgroundTaskDbContext(DbContextOptions<BackgroundTaskDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BackgroundJobEntity>(b =>
        {
            b.ToTable("BackgroundJobs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.CronExpression).HasMaxLength(128);
            b.Property(x => x.ApiUrl).HasMaxLength(1000);
            b.Property(x => x.ApiHttpMethod).HasMaxLength(16);
            b.Property(x => x.ApiHeaders).HasColumnType("nvarchar(max)");
            b.Property(x => x.ApiBody).HasColumnType("nvarchar(max)");
            b.Property(x => x.SqlConnectionString).HasMaxLength(1000);
            b.Property(x => x.SqlStatement).HasColumnType("nvarchar(max)");
            b.Property(x => x.HangfireJobId).HasMaxLength(128);
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.Name);
            b.HasIndex(x => x.TriggerKind);
            b.HasIndex(x => x.ExecuteType);
            b.HasIndex(x => x.IsEnabled);
        });

        modelBuilder.Entity<JobExecutionRecordEntity>(b =>
        {
            b.ToTable("JobExecutionRecords");
            b.HasKey(x => x.Id);
            b.Property(x => x.JobName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Result).HasColumnType("nvarchar(max)");
            b.Property(x => x.ErrorMessage).HasColumnType("nvarchar(max)");

            b.HasIndex(x => x.JobId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.StartTime);
        });
    }
}
