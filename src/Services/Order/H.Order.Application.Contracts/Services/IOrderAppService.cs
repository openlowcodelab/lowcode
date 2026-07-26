using H.Abp.Application.Contracts;

namespace H.Order.Application.Contracts;

/// <summary>
/// 订单应用服务接口（对外开放）。
/// ABP 约定控制器将自动生成 RESTful 端点：
///  - GET    /api/order/order            列表
///  - GET    /api/order/order/{id}       核心 DTO
///  - POST   /api/order/order            创建
///  - PUT    /api/order/order/{id}       更新
///  - DELETE /api/order/order/{id}       删除
///  - POST   /api/order/order/{id}/trigger-dispatch   手动触发下发
///  - GET    /api/order/order/{id}/dispatch-status     查询下发状态
///  - GET    /api/order/order/{id}/detail              详情（含扩展属性）
/// </summary>
public interface IOrderAppService : IAppService
{
    /// <summary>分页查询订单（仅核心字段，不关联扩展表）</summary>
    Task<PagedResultDto<OrderDto>> GetListAsync(OrderQueryDto input);

    /// <summary>按ID获取订单核心 DTO（不含扩展属性）</summary>
    Task<OrderDto> GetAsync(Guid id);

    /// <summary>获取订单详情（含行业扩展属性及最近下发状态）</summary>
    Task<OrderDetailDto> GetDetailAsync(Guid id);

    /// <summary>创建订单（核心字段 + 扩展属性同事务写入；若进入待下发则发布 CAP 事件）</summary>
    Task<OrderDto> CreateAsync(CreateOrderDto input);

    /// <summary>更新订单</summary>
    Task<OrderDto> UpdateAsync(Guid id, UpdateOrderDto input);

    /// <summary>删除订单（同步删除扩展属性）</summary>
    Task DeleteAsync(Guid id);

    /// <summary>手动触发订单下发到上游供应商</summary>
    Task<TriggerDispatchResultDto> TriggerDispatchAsync(Guid id);

    /// <summary>查询订单最近一次下发状态</summary>
    Task<DispatchStatusDto?> GetDispatchStatusAsync(Guid id);
}