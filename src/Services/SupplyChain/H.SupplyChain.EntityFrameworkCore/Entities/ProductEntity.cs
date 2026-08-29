using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.SupplyChain.EntityFrameworkCore;

/// <summary>
/// 商品主表（存储商品基本信息）
/// </summary>
public class ProductEntity : AuditedEntity<long>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>商品编码（唯一）</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>商品名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>商品类别</summary>
    public string? Category { get; set; }

    /// <summary>商品描述</summary>
    public string? Description { get; set; }

    /// <summary>商品状态</summary>
    public int Status { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
