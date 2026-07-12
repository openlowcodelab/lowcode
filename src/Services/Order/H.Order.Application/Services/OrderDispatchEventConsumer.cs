using DotNetCore.CAP;
using H.Order.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace H.Order.Application.Services;

/// <summary>
/// 订单待下发事件的 CAP 消费者。
/// 收到事件后调用 <see cref="IDispatchService.DispatchAsync"/> 完成供应商匹配与下发。
/// CAP 失败重试由框架内置（FailedRetryCount/FailedRetryInterval）。
/// </summary>
public class OrderDispatchEventConsumer : ICapSubscribe
{
    private readonly IDispatchService _dispatchService;
    private readonly ILogger<OrderDispatchEventConsumer> _logger;

    public OrderDispatchEventConsumer(
        IDispatchService dispatchService,
        ILogger<OrderDispatchEventConsumer> logger)
    {
        _dispatchService = dispatchService;
        _logger = logger;
    }

    [CapSubscribe(OrderTopics.PendingDispatch, Group = "order-dispatch-group")]
    public async Task HandleAsync(OrderPendingDispatchEvent evt, CancellationToken cancellationToken)
    {
        _logger.LogInformation("收到订单下发事件 OrderId={OrderId} OrderNo={OrderNo}", evt.OrderId, evt.OrderNo);
        var result = await _dispatchService.DispatchAsync(evt.OrderId);
        _logger.LogInformation("订单 {OrderId} 下发结果 Success={Success} Supplier={SupplierCode} Message={Message}",
            evt.OrderId, result.Success, result.SupplierCode, result.Message);
    }
}