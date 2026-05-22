using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Agent.EntityFrameworkCore;

[ConnectionStringName("AgentDb")]
public class AgentDbContext : AbpDbContext<AgentDbContext>
{
    public DbSet<LLMConfigEntity> LLMConfigs { get; set; } = null!;

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
            
            b.HasIndex(x => x.ProviderName).IsUnique();
        });
    }
}
