namespace H.Order.Application.Contracts;

/// <summary>
/// CAP 主题常量
/// </summary>
public static class OrderTopics
{
    /// <summary>订单进入待下发状态</summary>
    public const string PendingDispatch = "order.pending-dispatch";
}