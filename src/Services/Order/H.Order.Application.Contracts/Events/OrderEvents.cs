namespace H.Order.Application.Contracts;

/// <summary>
/// 订单转入待下发状态时发布的领域事件。
/// 由 CAP 投递给 <c>OrderDispatchEventConsumer</c> 触发供应商下发。
/// </summary>
public class OrderPendingDispatchEvent
{
    /// <summary>订单ID</summary>
    public Guid OrderId { get; set; }

    /// <summary>订单号</summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>行业</summary>
    public string? Industry { get; set; }

    /// <summary>商品类别</summary>
    public string? ProductCategory { get; set; }
}