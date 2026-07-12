using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace H.Order.EntityFrameworkCore;

/// <summary>
/// 订单应用数据库上下文
/// </summary>
[ConnectionStringName("OrderDb")]
public class OrderDbContext : AbpDbContext<OrderDbContext>
{
    /// <summary>订单核心表</summary>
    public DbSet<OrderEntity> Orders { get; set; } = null!;

    /// <summary>订单扩展表（按行业存储特有属性）</summary>
    public DbSet<OrderExtensionEntity> OrderExtensions { get; set; } = null!;

    /// <summary>供应商定义</summary>
    public DbSet<SupplierEntity> Suppliers { get; set; } = null!;

    /// <summary>路由规则</summary>
    public DbSet<RouteRuleEntity> RouteRules { get; set; } = null!;

    /// <summary>下发日志</summary>
    public DbSet<DispatchLogEntity> DispatchLogs { get; set; } = null!;

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderEntity>(b =>
        {
            b.ToTable("Orders");
            b.HasKey(x => x.Id);
            b.Property(x => x.OrderNo).IsRequired().HasMaxLength(64);
            b.Property(x => x.ProductName).IsRequired().HasMaxLength(256);
            b.Property(x => x.BuyerId).IsRequired().HasMaxLength(64);
            b.Property(x => x.Industry).HasMaxLength(64);
            b.Property(x => x.ProductCategory).HasMaxLength(128);
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.OrderNo).IsUnique();
            b.HasIndex(x => x.Industry);
            b.HasIndex(x => x.BuyerId);
            b.HasIndex(x => x.OrderStatus);
        });

        modelBuilder.Entity<OrderExtensionEntity>(b =>
        {
            b.ToTable("OrderExtensions");
            b.HasKey(x => x.Id);
            b.Property(x => x.AttributesJson).HasColumnType("nvarchar(max)");

            b.HasIndex(x => x.OrderId).IsUnique();
            b.HasOne(x => x.Order)
                .WithOne()
                .HasForeignKey<OrderExtensionEntity>(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

        modelBuilder.Entity<RouteRuleEntity>(b =>
        {
            b.ToTable("RouteRules");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.SupplierCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.ConditionsJson).HasColumnType("nvarchar(max)");
            b.Property(x => x.Remark).HasMaxLength(500);

            b.HasIndex(x => x.SupplierCode);
            b.HasIndex(x => x.IsEnabled);
        });

        modelBuilder.Entity<DispatchLogEntity>(b =>
        {
            b.ToTable("DispatchLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.SupplierCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.RequestPayload).HasColumnType("nvarchar(max)");
            b.Property(x => x.ResponsePayload).HasColumnType("nvarchar(max)");
            b.Property(x => x.ErrorMessage).HasMaxLength(2000);

            b.HasIndex(x => x.OrderId);
            b.HasIndex(x => x.SupplierCode);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.NextRetryTime);
        });
    }
}