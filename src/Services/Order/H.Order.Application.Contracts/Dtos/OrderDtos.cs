using H.Abstractions;

namespace H.Order.Application.Contracts;

/// <summary>
/// 订单列表/核心 DTO（仅包含所有行业共有的最小属性集，不含扩展属性）
/// </summary>
public class OrderDto : FullAuditedEntityDto<Guid>
{
    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 商品名称
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// 买家ID
    /// </summary>
    public string BuyerId { get; set; } = string.Empty;

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStatusEnum OrderStatus { get; set; }

    /// <summary>
    /// 行业（自由字符串，如 服装/餐饮/数码）
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// 商品类别（自由字符串）
    /// </summary>
    public string? ProductCategory { get; set; }

    /// <summary>
    /// 总金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 订单详情 DTO（核心信息 + 行业特有扩展属性 JSON）
/// </summary>
public class OrderDetailDto : OrderDto
{
    /// <summary>
    /// 行业特有属性（JSON 字符串），由扩展表取出
    /// </summary>
    public string? AttributesJson { get; set; }

    /// <summary>
    /// 最近一次下发状态
    /// </summary>
    public DispatchStatusDto? DispatchStatus { get; set; }
}

/// <summary>
/// 创建订单 DTO
/// </summary>
public class CreateOrderDto
{
    /// <summary>订单号（为空时由系统生成）</summary>
    public string? OrderNo { get; set; }

    /// <summary>商品名称</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>买家ID</summary>
    public string BuyerId { get; set; } = string.Empty;

    /// <summary>订单状态（默认待下发）</summary>
    public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.PendingDispatch;

    /// <summary>行业</summary>
    public string? Industry { get; set; }

    /// <summary>商品类别</summary>
    public string? ProductCategory { get; set; }

    /// <summary>总金额</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    /// <summary>行业特有属性（JSON 字符串），存入扩展表</summary>
    public string? AttributesJson { get; set; }
}

/// <summary>
/// 更新订单 DTO
/// </summary>
public class UpdateOrderDto
{
    /// <summary>商品名称</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>买家ID</summary>
    public string BuyerId { get; set; } = string.Empty;

    /// <summary>订单状态</summary>
    public OrderStatusEnum OrderStatus { get; set; }

    /// <summary>行业</summary>
    public string? Industry { get; set; }

    /// <summary>商品类别</summary>
    public string? ProductCategory { get; set; }

    /// <summary>总金额</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    /// <summary>行业特有属性（JSON 字符串），更新扩展表</summary>
    public string? AttributesJson { get; set; }
}

/// <summary>
/// 订单列表查询参数（分页 + 通用字段筛选）
/// </summary>
public class OrderQueryDto : PagedResultRequestDto
{
    /// <summary>搜索关键词（订单号或商品名称）</summary>
    public string? Filter { get; set; }

    /// <summary>精确订单号</summary>
    public string? OrderNo { get; set; }

    /// <summary>行业</summary>
    public string? Industry { get; set; }

    /// <summary>买家ID</summary>
    public string? BuyerId { get; set; }

    /// <summary>订单状态</summary>
    public OrderStatusEnum? Status { get; set; }

    /// <summary>最小金额</summary>
    public decimal? MinAmount { get; set; }

    /// <summary>最大金额</summary>
    public decimal? MaxAmount { get; set; }

    /// <summary>创建时间起</summary>
    public DateTime? CreateTimeStart { get; set; }

    /// <summary>创建时间止</summary>
    public DateTime? CreateTimeEnd { get; set; }
}