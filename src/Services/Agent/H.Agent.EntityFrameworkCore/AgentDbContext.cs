using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Agent.EntityFrameworkCore;

[ConnectionStringName("AgentDb")]
public class AgentDbContext : AbpDbContext<AgentDbContext>
{
    public DbSet<LLMConfigEntity> LLMConfigs { get; set; } = null!;
    public DbSet<AgentChatSessionEntity> ChatSessions { get; set; } = null!;
    public DbSet<AgentChatMessageEntity> ChatMessages { get; set; } = null!;

    public AgentDbContext(DbContextOptions<AgentDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LLMConfigEntity>(b =>
        {
            b.ToTable("AgentLLMConfigs");
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

        modelBuilder.Entity<AgentChatSessionEntity>(b =>
        {
            b.ToTable("AgentChatSessions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.AgentType).IsRequired().HasMaxLength(50);
            
            b.HasMany(x => x.Messages)
             .WithOne()
             .HasForeignKey(x => x.SessionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgentChatMessageEntity>(b =>
        {
            b.ToTable("AgentChatMessages");
            b.HasKey(x => x.Id);
            b.Property(x => x.SessionId).IsRequired();
            b.Property(x => x.Role).IsRequired().HasMaxLength(20);
            b.Property(x => x.Content).IsRequired().HasMaxLength(8000);
            b.Property(x => x.ToolName).HasMaxLength(100);
            b.Property(x => x.ToolResult).HasMaxLength(2000);
            
            b.HasIndex(x => x.SessionId);
        });
    }
}
