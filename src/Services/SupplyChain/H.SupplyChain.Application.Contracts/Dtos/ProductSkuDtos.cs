using H.Abp.Application.Contracts;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 商品 SKU DTO（商品最小可售卖单元）
/// </summary>
public class ProductSkuDto : AuditedEntityDto<long>
{
    /// <summary>商品ID</summary>
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

/// <summary>
/// 创建商品 SKU DTO
/// </summary>
public class CreateProductSkuDto
{
    /// <summary>商品ID</summary>
    public long ProductId { get; set; }

    /// <summary>SKU 编码</summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>SKU 名称</summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>规格属性（JSON）</summary>
    public string? SpecsJson { get; set; }

    /// <summary>售价</summary>
    public decimal Price { get; set; }

    /// <summary>库存</summary>
    public int Stock { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新商品 SKU DTO
/// </summary>
public class UpdateProductSkuDto
{
    /// <summary>SKU 名称</summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>规格属性（JSON）</summary>
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

/// <summary>
/// 商品 SKU 查询参数
/// </summary>
public class ProductSkuQueryDto : PagedResultRequestDto
{
    /// <summary>商品ID</summary>
    public long? ProductId { get; set; }

    /// <summary>关键词（编码或名称）</summary>
    public string? Filter { get; set; }

    /// <summary>是否启用</summary>
    public bool? IsEnabled { get; set; }
}