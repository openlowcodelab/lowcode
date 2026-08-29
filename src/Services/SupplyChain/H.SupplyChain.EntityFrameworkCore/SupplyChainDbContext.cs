using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.SupplyChain.EntityFrameworkCore;

/// <summary>
/// 供应链集成应用数据库上下文
/// </summary>
[ConnectionStringName("SupplyChainDb")]
public class SupplyChainDbContext : AbpDbContext<SupplyChainDbContext>
{
    /// <summary>供应商定义</summary>
    public DbSet<SupplierEntity> Suppliers { get; set; } = null!;

    /// <summary>商品主表</summary>
    public DbSet<ProductEntity> Products { get; set; } = null!;

    /// <summary>商品 SKU 表</summary>
    public DbSet<ProductSkuEntity> ProductSkus { get; set; } = null!;

    /// <summary>供应商 SKU 映射表</summary>
    public DbSet<SupplierSkuMappingEntity> SupplierSkuMappings { get; set; } = null!;

    /// <summary>接口定义</summary>
    public DbSet<ApiInterfaceEntity> ApiInterfaces { get; set; } = null!;

    /// <summary>供应商接口映射</summary>
    public DbSet<SupplierInterfaceMappingEntity> SupplierInterfaceMappings { get; set; } = null!;

    public SupplyChainDbContext(DbContextOptions<SupplyChainDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SupplierEntity>(b =>
        {
            b.ToTable("Suppliers");
            b.HasKey(x => x.Id);

            b.Property(x => x.Code).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.DisplayName).HasMaxLength(128);
            b.Property(x => x.ApiUrl).HasMaxLength(500);
            b.Property(x => x.AuthConfig).HasColumnType("nvarchar(max)");
            b.Property(x => x.ProtocolConfig).HasColumnType("nvarchar(max)");
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<ProductEntity>(b =>
        {
            b.ToTable("Products");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(10000, 1);
            b.Property(x => x.ProductCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(256);
            b.Property(x => x.Category).HasMaxLength(128);
            b.Property(x => x.Description).HasColumnType("nvarchar(max)");
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.ProductCode).IsUnique();
            b.HasIndex(x => x.Category);
            b.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<ProductSkuEntity>(b =>
        {
            b.ToTable("ProductSkus");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(200000, 1);
            b.Property(x => x.SkuCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.SkuName).IsRequired().HasMaxLength(256);
            b.Property(x => x.SpecsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Price).HasColumnType("decimal(18,2)");
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.SkuCode).IsUnique();
            b.HasIndex(x => x.ProductId);
        });

        modelBuilder.Entity<SupplierSkuMappingEntity>(b =>
        {
            b.ToTable("SupplierSkuMappings");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(3000000, 1);
            b.Property(x => x.SupplierSkuCode).IsRequired().HasMaxLength(128);
            b.Property(x => x.SupplierSkuName).HasMaxLength(256);
            b.Property(x => x.SupplierPrice).HasColumnType("decimal(18,2)");
            b.Property(x => x.Remark).HasMaxLength(500);

            // 一个内部 SKU 可映射多个供应商：同一 SkuId + SupplierId 唯一
            b.HasIndex(x => new { x.SkuId, x.SupplierId }).IsUnique();
            b.HasIndex(x => x.SkuId);
            b.HasIndex(x => x.SupplierId);
        });

        modelBuilder.Entity<ApiInterfaceEntity>(b =>
        {
            b.ToTable("ApiInterfaces");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(100000, 1);
            b.Property(x => x.Code).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.HttpMethod).HasMaxLength(16);
            b.Property(x => x.Path).HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.RequestFieldsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.ResponseFieldsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.Code).IsUnique();
            b.HasIndex(x => x.InterfaceType);
        });

        modelBuilder.Entity<SupplierInterfaceMappingEntity>(b =>
        {
            b.ToTable("SupplierInterfaceMappings");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).UseIdentityColumn(100000, 1);
            b.Property(x => x.SupplierApiUrl).HasMaxLength(500);
            b.Property(x => x.RequestMappingJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.ResponseMappingJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Remark).HasMaxLength(500);

            // 一个供应商对同一接口仅维护一份映射
            b.HasIndex(x => new { x.SupplierId, x.InterfaceId }).IsUnique();
            b.HasIndex(x => x.SupplierId);
            b.HasIndex(x => x.InterfaceId);
        });
    }
}