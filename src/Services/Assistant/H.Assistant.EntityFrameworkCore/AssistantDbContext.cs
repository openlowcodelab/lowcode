using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Assistant.EntityFrameworkCore;

[ConnectionStringName("AssistantDb")]
public class AssistantDbContext : AbpDbContext<AssistantDbContext>
{
    public DbSet<LLMEntity> LLMConfigs { get; set; } = null!;
    public DbSet<ChatEntity> ChatSessions { get; set; } = null!;
    public DbSet<ChatMessageEntity> ChatMessages { get; set; } = null!;
    public DbSet<TaskEntity> ScheduledTasks { get; set; } = null!;
    public DbSet<TaskLogEntity> TaskExecutionLogs { get; set; } = null!;
    public DbSet<AgentEntity> AgentDefinitions { get; set; } = null!;
    public DbSet<SkillEntity> SkillDefinitions { get; set; } = null!;

    public AssistantDbContext(DbContextOptions<AssistantDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LLMEntity>(b =>
        {
            b.ToTable("LLMConfigs");
            b.HasKey(x => x.Id);
            b.Property(x => x.ProviderName).IsRequired().HasMaxLength(50);
            b.Property(x => x.ProviderDisplayName).HasMaxLength(100);
            b.Property(x => x.ApiKey).IsRequired().HasMaxLength(500);
            b.Property(x => x.ApiSecret).HasMaxLength(500);
            b.Property(x => x.BaseUrl).HasMaxLength(500);
            b.Property(x => x.Model).IsRequired().HasMaxLength(100);
            b.Property(x => x.ExtraConfig).HasMaxLength(2000);
            
            b.HasIndex(x => new { x.ProviderName, x.Model }).IsUnique();
        });

        modelBuilder.Entity<ChatEntity>(b =>
        {
            b.ToTable("ChatSessions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.AgentType).IsRequired().HasMaxLength(50);
            
            b.HasMany(x => x.Messages)
             .WithOne()
             .HasForeignKey(x => x.SessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessageEntity>(b =>
        {
            b.ToTable("ChatMessages");
            b.HasKey(x => x.Id);
            b.Property(x => x.SessionId).IsRequired();
            b.Property(x => x.Role).IsRequired().HasMaxLength(20);
            b.Property(x => x.Content).IsRequired().HasMaxLength(8000);
            b.Property(x => x.ToolName).HasMaxLength(100);
            b.Property(x => x.ToolResult).HasMaxLength(2000);
            
            b.HasIndex(x => x.SessionId);
        });

        modelBuilder.Entity<TaskEntity>(b =>
        {
            b.ToTable("ScheduledTasks");
            b.HasKey(x => x.Id);
            b.Property(x => x.TaskName).IsRequired().HasMaxLength(100);
            b.Property(x => x.TaskDescription).HasMaxLength(500);
            b.Property(x => x.TaskType).IsRequired().HasMaxLength(50);
            b.Property(x => x.PromptContent).IsRequired().HasMaxLength(8000);
            b.Property(x => x.AgentType).IsRequired().HasMaxLength(50);
            b.Property(x => x.ScheduleType).IsRequired().HasMaxLength(20);
            b.Property(x => x.CronExpression).HasMaxLength(100);
            b.Property(x => x.Status).IsRequired().HasMaxLength(20);
            b.Property(x => x.HangfireJobId).HasMaxLength(100);
            
            b.HasIndex(x => new { x.IsEnabled, x.Status });
            b.HasIndex(x => x.NextExecutionTime);
        });

        modelBuilder.Entity<TaskLogEntity>(b =>
        {
            b.ToTable("TaskExecutionLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.TaskId).IsRequired();
            b.Property(x => x.Status).IsRequired().HasMaxLength(20);
            b.Property(x => x.Result).HasMaxLength(8000);
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);
            
            b.HasIndex(x => x.TaskId);
            b.HasIndex(x => x.StartTime);
        });

        modelBuilder.Entity<AgentEntity>(b =>
        {
            b.ToTable("AgentDefinitions");
            b.HasKey(x => x.Id);
            b.Property(x => x.AgentType).IsRequired().HasMaxLength(100);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.SystemPrompt).IsRequired().HasMaxLength(4000);
            b.Property(x => x.Metadata).HasMaxLength(4000);
            b.Property(x => x.SkillIds).HasMaxLength(2000);
            
            b.HasIndex(x => x.AgentType).IsUnique();
            b.HasIndex(x => x.IsEnabled);
        });

        modelBuilder.Entity<SkillEntity>(b =>
        {
            b.ToTable("SkillDefinitions");
            b.HasKey(x => x.Id);
            b.Property(x => x.SkillName).IsRequired().HasMaxLength(100);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.SkillType).IsRequired().HasMaxLength(50);
            b.Property(x => x.ImplementationClass).HasMaxLength(500);
            b.Property(x => x.Config).HasMaxLength(4000);
            b.Property(x => x.ParameterSchema).HasMaxLength(4000);
            
            b.HasIndex(x => x.SkillName).IsUnique();
            b.HasIndex(x => x.IsEnabled);
            b.HasIndex(x => x.SkillType);
        });
    }
}
