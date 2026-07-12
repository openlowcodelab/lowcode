namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 菜单查询参数（对外 API）
/// </summary>
public class MenuQueryDto
{
    /// <summary>供应商编码（指定要查询哪个供应商的菜单，为空则返回内部商品目录）</summary>
    public string? SupplierCode { get; set; }

    /// <summary>商品类别</summary>
    public string? Category { get; set; }

    /// <summary>关键词</summary>
    public string? Filter { get; set; }

    /// <summary>最大返回条数</summary>
    public int MaxResultCount { get; set; } = 100;
}

/// <summary>
/// 菜单项（对外 API 返回）
/// </summary>
public class MenuItemDto
{
    /// <summary>商品ID</summary>
    public Guid ProductId { get; set; }

    /// <summary>商品编码</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>商品名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>商品类别</summary>
    public string? Category { get; set; }

    /// <summary>商品描述</summary>
    public string? Description { get; set; }

    /// <summary>商品状态</summary>
    public ProductStatusEnum Status { get; set; }

    /// <summary>SKU 列表</summary>
    public List<MenuSkuDto> Skus { get; set; } = new();
}

/// <summary>
/// 菜单中的 SKU 项
/// </summary>
public class MenuSkuDto
{
    /// <summary>SKU 编码</summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>SKU 名称</summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>售价</summary>
    public decimal Price { get; set; }

    /// <summary>库存</summary>
    public int Stock { get; set; }

    /// <summary>供应商侧 SKU 编码（来自映射，用于向该供应商下单）</summary>
    public string? SupplierSkuCode { get; set; }
}

/// <summary>
/// 菜单结果
/// </summary>
public class MenuResultDto
{
    /// <summary>供应商编码（若指定）</summary>
    public string? SupplierCode { get; set; }

    /// <summary>菜单项列表</summary>
    public List<MenuItemDto> Items { get; set; } = new();
}

/// <summary>
/// 商品详情查询参数（对外 API）
/// </summary>
public class ProductDetailQueryDto
{
    /// <summary>商品编码</summary>
    public string? ProductCode { get; set; }

    /// <summary>SKU 编码</summary>
    public string? SkuCode { get; set; }

    /// <summary>供应商编码（指定向哪个供应商查询详情，为空则返回内部详情）</summary>
    public string? SupplierCode { get; set; }
}

/// <summary>
/// 对外商品详情结果（内部详情 + 可选的供应商侧字段）
/// </summary>
public class ProductDetailResultDto : ProductDetailDto
{
    /// <summary>供应商编码（若指定）</summary>
    public string? SupplierCode { get; set; }

    /// <summary>供应商返回的原始字段（按 ResponseMapping 解析后的标准字段）</summary>
    public Dictionary<string, string?> SupplierFields { get; set; } = new();
}

/// <summary>
/// 下单 DTO（对外 API）
/// </summary>
public class PlaceOrderDto
{
    /// <summary>目标供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>内部 SKU 编码</summary>
    public string SkuCode { get; set; } = string.Empty;

    /// <summary>下单数量</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>外部订单号（调用方业务单号）</summary>
    public string? ExternalOrderNo { get; set; }

    /// <summary>收货人</summary>
    public string? Receiver { get; set; }

    /// <summary>收货地址</summary>
    public string? Address { get; set; }

    /// <summary>联系电话</summary>
    public string? Phone { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 下单结果
/// </summary>
public class PlaceOrderResultDto
{
    /// <summary>是否成功</summary>
    public bool Success { get; set; }

    /// <summary>状态</summary>
    public OrderPlaceStatusEnum Status { get; set; }

    /// <summary>供应商编码</summary>
    public string? SupplierCode { get; set; }

    /// <summary>供应商返回的订单号（按 ResponseMapping 解析）</summary>
    public string? SupplierOrderNo { get; set; }

    /// <summary>提示信息</summary>
    public string? Message { get; set; }

    /// <summary>供应商原始应答</summary>
    public string? RawResponse { get; set; }

    /// <summary>解析后的标准字段</summary>
    public Dictionary<string, string?> MappedFields { get; set; } = new();
}