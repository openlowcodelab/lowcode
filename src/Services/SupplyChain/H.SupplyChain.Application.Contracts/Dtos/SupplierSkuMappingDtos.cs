using H.Abp.Application.Contracts;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 供应商 SKU 映射 DTO。
/// 一个内部 SKU 可映射多个供应商，用于向不同供应商下单。
/// </summary>
public class SupplierSkuMappingDto : FullAuditedEntityDto<Guid>
{
    /// <summary>内部 SKU ID</summary>
    public Guid SkuId { get; set; }

    /// <summary>内部 SKU 编码（冗余便于展示）</summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>供应商ID</summary>
    public Guid SupplierId { get; set; }

    /// <summary>供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

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
}

/// <summary>
/// 创建供应商 SKU 映射 DTO
/// </summary>
public class CreateSupplierSkuMappingDto
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
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新供应商 SKU 映射 DTO
/// </summary>
public class UpdateSupplierSkuMappingDto
{
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
}

/// <summary>
/// 供应商 SKU 映射查询参数
/// </summary>
public class SupplierSkuMappingQueryDto : PagedResultRequestDto
{
    /// <summary>内部 SKU ID</summary>
    public Guid? SkuId { get; set; }

    /// <summary>供应商ID</summary>
    public Guid? SupplierId { get; set; }

    /// <summary>是否启用</summary>
    public bool? IsEnabled { get; set; }
}