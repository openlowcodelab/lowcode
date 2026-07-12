namespace H.Order.Application.Contracts;

/// <summary>
/// 供应商对接统一接口。新增 HTTP / Mock / MQ / gRPC 等协议时只需新增一个实现并在工厂中注册。
/// </summary>
public interface ISupplierClient
{
    /// <summary>该实现支持的协议</summary>
    SupplierProtocolEnum Protocol { get; }

    /// <summary>
    /// 调用供应商接口将订单下发过去
    /// </summary>
    Task<SupplierResponse> SendAsync(SupplierContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// 供应商客户端工厂：按协议返回对应的 <see cref="ISupplierClient"/> 实现
/// </summary>
public interface ISupplierClientFactory
{
    /// <summary>根据协议枚举获取实现</summary>
    ISupplierClient Get(SupplierProtocolEnum protocol);
}

/// <summary>
/// 下发上下文：供应商定义 + 规范化的订单下发载荷
/// </summary>
public class SupplierContext
{
    /// <summary>供应商信息</summary>
    public SupplierInfo Supplier { get; set; } = new();

    /// <summary>下发给供应商的订单载荷</summary>
    public OrderDispatchPayload Payload { get; set; } = new();
}

/// <summary>
/// 供应商信息（脱敏后传给 ISupplierClient，避免直接操作实体）
/// </summary>
public class SupplierInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ApiUrl { get; set; }
    public AuthTypeEnum AuthType { get; set; }
    public string? AuthConfig { get; set; }
    public SupplierProtocolEnum Protocol { get; set; }
}

/// <summary>
/// 下发给供应商的规范化订单载荷（核心字段 + 行业扩展 JSON）
/// </summary>
public class OrderDispatchPayload
{
    public string OrderNo { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string BuyerId { get; set; } = string.Empty;
    public OrderStatusEnum OrderStatus { get; set; }
    public string? Industry { get; set; }
    public string? ProductCategory { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Remark { get; set; }

    /// <summary>行业特有属性 JSON</summary>
    public string? AttributesJson { get; set; }
}

/// <summary>
/// 供应商调用结果
/// </summary>
public class SupplierResponse
{
    public bool Success { get; set; }

    public int? StatusCode { get; set; }

    public string? ResponseBody { get; set; }

    public string? ErrorMessage { get; set; }

    public static SupplierResponse Ok(int statusCode, string? body) => new()
    {
        Success = true,
        StatusCode = statusCode,
        ResponseBody = body
    };

    public static SupplierResponse Fail(int? statusCode, string? body, string error) => new()
    {
        Success = false,
        StatusCode = statusCode,
        ResponseBody = body,
        ErrorMessage = error
    };
}