namespace H.Order.Application.Contracts;

/// <summary>
/// 订单状态：草稿、待下发、已下发、已完成、已取消
/// </summary>
public enum OrderStatusEnum
{
    /// <summary>草稿</summary>
    Draft = 0,

    /// <summary>待下发</summary>
    PendingDispatch = 1,

    /// <summary>已下发</summary>
    Dispatched = 2,

    /// <summary>已完成</summary>
    Completed = 3,

    /// <summary>已取消</summary>
    Cancelled = 4
}

/// <summary>
/// 下发状态：待下发、成功、失败、重试中
/// </summary>
public enum DispatchStatusEnum
{
    /// <summary>待下发</summary>
    Pending = 0,

    /// <summary>成功</summary>
    Success = 1,

    /// <summary>失败</summary>
    Failed = 2,

    /// <summary>重试中</summary>
    Retrying = 3
}

/// <summary>
/// 供应商对接协议：HTTP 调用 / Mock 模拟
/// </summary>
public enum SupplierProtocolEnum
{
    /// <summary>HTTP 协议</summary>
    Http = 0,

    /// <summary>模拟协议（不实际调用外部）</summary>
    Mock = 1
}

/// <summary>
/// 供应商认证方式
/// </summary>
public enum AuthTypeEnum
{
    /// <summary>无需认证</summary>
    None = 0,

    /// <summary>ApiKey（query 参数或头）</summary>
    ApiKey = 1,

    /// <summary>自定义请求头</summary>
    Header = 2,

    /// <summary>Basic 认证</summary>
    Basic = 3,

    /// <summary>Bearer Token</summary>
    Bearer = 4
}

/// <summary>
/// 路由规则类型：按行业 / 按商品类别 / 按金额区间 / 自定义组合
/// </summary>
public enum RouteRuleTypeEnum
{
    /// <summary>按行业匹配</summary>
    Industry = 0,

    /// <summary>按商品类别匹配</summary>
    Category = 1,

    /// <summary>按金额区间匹配</summary>
    AmountRange = 2,

    /// <summary>自定义组合条件</summary>
    Custom = 3
}