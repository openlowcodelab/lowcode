using H.Abstractions;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 商品主表 DTO（商品基本信息）
/// </summary>
public class ProductDto : FullAuditedEntityDto<Guid>
{
    /// <summary>商品编码（唯一）</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>商品名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>商品类别</summary>
    public string? Category { get; set; }

    /// <summary>商品描述</summary>
    public string? Description { get; set; }

    /// <summary>商品状态</summary>
    public ProductStatusEnum Status { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 商品详情 DTO（主表 + SKU 列表 + 各 SKU 可用供应商映射）
/// </summary>
public class ProductDetailDto : ProductDto
{
    /// <summary>商品 SKU 列表</summary>
    public List<ProductSkuDto> Skus { get; set; } = new();
}

/// <summary>
/// 创建商品 DTO
/// </summary>
public class CreateProductDto
{
    /// <summary>商品编码</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>商品名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>商品类别</summary>
    public string? Category { get; set; }

    /// <summary>商品描述</summary>
    public string? Description { get; set; }

    /// <summary>商品状态</summary>
    public ProductStatusEnum Status { get; set; } = ProductStatusEnum.OnShelf;

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新商品 DTO
/// </summary>
public class UpdateProductDto
{
    /// <summary>商品名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>商品类别</summary>
    public string? Category { get; set; }

    /// <summary>商品描述</summary>
    public string? Description { get; set; }

    /// <summary>商品状态</summary>
    public ProductStatusEnum Status { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 商品查询参数
/// </summary>
public class ProductQueryDto : PagedResultRequestDto
{
    /// <summary>关键词（编码或名称）</summary>
    public string? Filter { get; set; }

    /// <summary>商品类别</summary>
    public string? Category { get; set; }

    /// <summary>商品状态</summary>
    public ProductStatusEnum? Status { get; set; }
}