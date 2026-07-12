using Volo.Abp.Application.Dtos;

namespace H.Order.Application.Contracts;

/// <summary>
/// 下发日志 DTO
/// </summary>
public class DispatchLogDto : FullAuditedEntityDto<Guid>
{
    /// <summary>订单ID</summary>
    public Guid OrderId { get; set; }

    /// <summary>供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>下发状态</summary>
    public DispatchStatusEnum Status { get; set; }

    /// <summary>尝试次数</summary>
    public int AttemptCount { get; set; }

    /// <summary>请求负载（JSON）</summary>
    public string? RequestPayload { get; set; }

    /// <summary>响应内容</summary>
    public string? ResponsePayload { get; set; }

    /// <summary>HTTP 状态码</summary>
    public int? StatusCode { get; set; }

    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>下次重试时间</summary>
    public DateTime? NextRetryTime { get; set; }

    /// <summary>请求时间</summary>
    public DateTime? RequestTime { get; set; }

    /// <summary>响应时间</summary>
    public DateTime? ResponseTime { get; set; }
}

/// <summary>
/// 下发日志查询参数
/// </summary>
public class DispatchLogQueryDto : PagedResultRequestDto
{
    /// <summary>订单ID</summary>
    public Guid? OrderId { get; set; }

    /// <summary>供应商编码</summary>
    public string? SupplierCode { get; set; }

    /// <summary>下发状态</summary>
    public DispatchStatusEnum? Status { get; set; }
}

/// <summary>
/// 手动触发下发结果
/// </summary>
public class TriggerDispatchResultDto
{
    /// <summary>订单ID</summary>
    public Guid OrderId { get; set; }

    /// <summary>下发的供应商编码（可能为空表示未匹配到供应商）</summary>
    public string? SupplierCode { get; set; }

    /// <summary>是否成功</summary>
    public bool Success { get; set; }

    /// <summary>提示信息</summary>
    public string? Message { get; set; }

    /// <summary>本次下发日志ID</summary>
    public Guid? LogId { get; set; }
}

/// <summary>
/// 订单下发状态摘要（详情接口附带的最新一条下发状态）
/// </summary>
public class DispatchStatusDto
{
    /// <summary>供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>下发状态</summary>
    public DispatchStatusEnum Status { get; set; }

    /// <summary>最近一次错误信息</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>最近一次请求时间</summary>
    public DateTime? RequestTime { get; set; }
}