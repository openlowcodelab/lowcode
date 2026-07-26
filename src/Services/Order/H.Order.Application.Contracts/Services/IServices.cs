using H.Abstractions;

namespace H.Order.Application.Contracts;

/// <summary>
/// 供应商管理接口
/// </summary>
public interface ISupplierAppService : ICrudAppService<SupplierDto, Guid, SupplierQueryDto, CreateSupplierDto, UpdateSupplierDto>
{
}

/// <summary>
/// 路由规则管理接口
/// </summary>
public interface IRouteRuleAppService : ICrudAppService<RouteRuleDto, Guid, RouteRuleQueryDto, CreateRouteRuleDto, UpdateRouteRuleDto>
{
}

/// <summary>
/// 下发日志查询接口
/// </summary>
public interface IDispatchLogAppService : IAppService
{
    /// <summary>分页查询下发日志</summary>
    Task<PagedResultDto<DispatchLogDto>> GetListAsync(DispatchLogQueryDto input);

    /// <summary>按订单ID获取最新下发日志</summary>
    Task<DispatchLogDto?> GetLatestByOrderIdAsync(Guid orderId);

    /// <summary>根据日志ID手动重试下发</summary>
    Task<TriggerDispatchResultDto> RetryAsync(Guid logId);
}

/// <summary>
/// 下发执行服务（非对外 HTTP 服务，供 OrderAppService 与 CAP 消费者复用）
/// </summary>
public interface IDispatchService
{
    /// <summary>
    /// 根据订单ID匹配供应商、调用外部接口、写入下发日志。
    /// </summary>
    Task<TriggerDispatchResultDto> DispatchAsync(Guid orderId);
}