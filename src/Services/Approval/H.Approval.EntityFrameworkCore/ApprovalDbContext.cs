using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace H.Approval.EntityFrameworkCore;

public class ApprovalDbContext : AbpDbContext<ApprovalDbContext>
{
    public ApprovalDbContext(DbContextOptions<ApprovalDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// 审批定义集合
    /// </summary>
    public virtual DbSet<ApprovalDefinition> ApprovalDefinitions { get; set; }
    
    /// <summary>
    /// 审批实例集合
    /// </summary>
    public virtual DbSet<ApprovalInstance> ApprovalInstances { get; set; }
    
    /// <summary>
    /// 审批任务集合
    /// </summary>
    public virtual DbSet<ApprovalTask> ApprovalTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // 配置审批定义表
        builder.Entity<ApprovalDefinition>(entity =>
        {
            entity.ToTable("ApprovalDefinitions");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1024);
            entity.Property(e => e.DefinitionJson).HasColumnType("nvarchar(max)");
        });
        
        // 配置审批实例表
        builder.Entity<ApprovalInstance>(entity =>
        {
            entity.ToTable("ApprovalInstances");
            entity.Property(e => e.DefinitionId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.DefinitionName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(512);
            entity.Property(e => e.CreatorId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.CreatorName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.CurrentNodeId).HasMaxLength(128);
            entity.Property(e => e.CurrentNodeName).HasMaxLength(256);
            entity.Property(e => e.VariablesJson).HasColumnType("nvarchar(max)");

            // 配置与任务的关系
            entity.HasMany(e => e.Tasks)
                .WithOne()
                .HasForeignKey(e => e.InstanceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 配置审批任务表
        builder.Entity<ApprovalTask>(entity =>
        {
            entity.ToTable("ApprovalTasks");
            entity.Property(e => e.InstanceId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.ApprovalName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.InstanceTitle).HasMaxLength(512);
            entity.Property(e => e.NodeId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.NodeName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.AssigneeId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.AssigneeName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Comment).HasMaxLength(1024);

            // 添加索引
            entity.HasIndex(e => e.AssigneeId);
            entity.HasIndex(e => e.InstanceId);
            entity.HasIndex(e => e.NodeId);
            entity.HasIndex(e => e.Status);
        });
        
        // Elsa 相关表由 Elsa.EntityFrameworkCore 自动配置
    }
}
