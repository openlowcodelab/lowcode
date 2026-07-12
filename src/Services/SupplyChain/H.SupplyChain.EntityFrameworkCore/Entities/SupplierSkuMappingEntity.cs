using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace H.SupplyChain.EntityFrameworkCore;

/// <summary>
/// 供应商 SKU 映射表。
/// 存储供应商对应的 SKU 值，支持一个内部 SKU 映射多个供应商，用于向供应商下单等场景。
/// </summary>
public class SupplierSkuMappingEntity : FullAuditedEntity<Guid>
{
    /// <summary>内部 SKU ID</summary>
    public Guid SkuId { get; set; }

    /// <summary>供应商ID</summary>
    public Guid SupplierId { get; set; }

    /// <summary>供应商商品编码</summary>
    public string SupplierSkuCode { get; set; } = string.Empty;

    /// <summary>供应商商品名称</summary>
    public string SupplierSkuName { get; set; } = string.Empty;

    /// <summary>供应商供货价格</summary>
    public decimal SupplierPrice { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    /// <summary>关联内部 SKU</summary>
    public virtual ProductSkuEntity? Sku { get; set; }

    /// <summary>关联供应商</summary>
    public virtual SupplierEntity? Supplier { get; set; }
}
