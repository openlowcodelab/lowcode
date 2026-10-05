using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.AI.EntityFrameworkCore;

[ConnectionStringName("AIDb")]
public class AIDbContext : AbpDbContext<AIDbContext>
{
    public DbSet<LLMEntity> Llms { get; set; } = null!;
    public DbSet<KnowledgeBaseEntity> KnowledgeBases { get; set; } = null!;
    public DbSet<KnowledgeNodeEntity> KnowledgeNodes { get; set; } = null!;
    public DbSet<KnowledgeDocumentEntity> KnowledgeDocuments { get; set; } = null!;

    public AIDbContext(DbContextOptions<AIDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LLMEntity>(b =>
        {
            b.ToTable("Llm");
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

        modelBuilder.Entity<KnowledgeBaseEntity>(b =>
        {
            b.ToTable("KnowledgeBase");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);

            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<KnowledgeNodeEntity>(b =>
        {
            b.ToTable("KnowledgeNode");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.NodeType).IsRequired().HasMaxLength(20);
            b.Property(x => x.OwnerType).IsRequired().HasMaxLength(20).HasDefaultValue("Knowledge");

            b.HasIndex(x => new { x.OwnerType, x.KnowledgeBaseId, x.ParentId });
        });

        modelBuilder.Entity<KnowledgeDocumentEntity>(b =>
        {
            b.ToTable("KnowledgeDocument");
            b.HasKey(x => x.Id);
            b.Property(x => x.NodeId);
            b.Property(x => x.Content).HasMaxLength(100000);
        });
    }
}
