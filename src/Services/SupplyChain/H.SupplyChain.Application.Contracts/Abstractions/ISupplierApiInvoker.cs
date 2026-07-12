namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 供应商接口调用统一接口。新增 HTTP / Mock / MQ / gRPC 等协议时只需新增一个实现并在工厂中注册。
/// </summary>
public interface ISupplierApiInvoker
{
    /// <summary>该实现支持的协议</summary>
    SupplierProtocolEnum Protocol { get; }

    /// <summary>
    /// 调用供应商接口：
    /// 1. 按 RequestMappings 将标准 Input 映射为供应商请求体；
    /// 2. 调用供应商接口；
    /// 3. 按 ResponseMappings 解析供应商应答，得到标准字段输出。
    /// </summary>
    Task<SupplierApiResponse> InvokeAsync(SupplierApiContext context, CancellationToken cancellationToken = default);
}

/// <summary>
/// 供应商接口调用工厂：按协议返回对应的 <see cref="ISupplierApiInvoker"/> 实现
/// </summary>
public interface ISupplierApiInvokerFactory
{
    /// <summary>根据协议枚举获取实现</summary>
    ISupplierApiInvoker Get(SupplierProtocolEnum protocol);
}

/// <summary>
/// 供应商接口调用结果
/// </summary>
public class SupplierApiResponse
{
    public bool Success { get; set; }

    public int? StatusCode { get; set; }

    public string? ResponseBody { get; set; }

    /// <summary>按 ResponseMappings 解析后的标准字段输出</summary>
    public Dictionary<string, string?> MappedFields { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public static SupplierApiResponse Ok(int statusCode, string? body, Dictionary<string, string?> mapped) => new()
    {
        Success = true,
        StatusCode = statusCode,
        ResponseBody = body,
        MappedFields = mapped
    };

    public static SupplierApiResponse Fail(int? statusCode, string? body, string error) => new()
    {
        Success = false,
        StatusCode = statusCode,
        ResponseBody = body,
        ErrorMessage = error
    };
}