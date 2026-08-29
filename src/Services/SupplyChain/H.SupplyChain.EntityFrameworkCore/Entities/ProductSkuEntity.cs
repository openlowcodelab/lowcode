using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.SupplyChain.EntityFrameworkCore;

/// <summary>
/// 商品 SKU 表（存储商品 SKU 信息）
/// </summary>
public class ProductSkuEntity : AuditedEntity<long>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>商品ID（多对一）</summary>
    public long ProductId { get; set; }

    /// <summary>SKU 编码（唯一）</summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>SKU 名称</summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>规格属性（JSON，如 {"color":"red","size":"XL"}）</summary>
    public string? SpecsJson { get; set; }

    /// <summary>售价</summary>
    public decimal Price { get; set; }

    /// <summary>库存</summary>
    public int Stock { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
