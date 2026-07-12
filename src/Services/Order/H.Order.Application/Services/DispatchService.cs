using System.Text.Json;
using DotNetCore.CAP;
using H.Order.Application.Contracts;
using H.Order.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace H.Order.Application.Services;

/// <summary>
/// 供应商下发执行服务：匹配路由规则 -> 抽象协议调用 -> 写入下发日志 -> 更新订单状态。
/// 供 OrderAppService 手动触发与 OrderDispatchEventConsumer 自动触发共同复用。
/// </summary>
public class DispatchService : IDispatchService
{
    private readonly IRepository<OrderEntity, Guid> _orderRepo;
    private readonly IRepository<OrderExtensionEntity, Guid> _extensionRepo;
    private readonly IRepository<SupplierEntity, Guid> _supplierRepo;
    private readonly IRepository<DispatchLogEntity, Guid> _logRepo;
    private readonly IRouteEngine _routeEngine;
    private readonly ISupplierClientFactory _clientFactory;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly ILogger<DispatchService> _logger;

    public DispatchService(
        IRepository<OrderEntity, Guid> orderRepo,
        IRepository<OrderExtensionEntity, Guid> extensionRepo,
        IRepository<SupplierEntity, Guid> supplierRepo,
        IRepository<DispatchLogEntity, Guid> logRepo,
        IRouteEngine routeEngine,
        ISupplierClientFactory clientFactory,
        IUnitOfWorkManager uowManager,
        ILogger<DispatchService> logger)
    {
        _orderRepo = orderRepo;
        _extensionRepo = extensionRepo;
        _supplierRepo = supplierRepo;
        _logRepo = logRepo;
        _routeEngine = routeEngine;
        _clientFactory = clientFactory;
        _uowManager = uowManager;
        _logger = logger;
    }

    public async Task<TriggerDispatchResultDto> DispatchAsync(Guid orderId)
    {
        var result = new TriggerDispatchResultDto { OrderId = orderId };

        using var uow = _uowManager.Begin();

        try
        {
            var order = await _orderRepo.FindAsync(orderId);
            if (order is null)
            {
                result.Success = false;
                result.Message = "订单不存在";
                return result;
            }

            if ((OrderStatusEnum)order.OrderStatus == OrderStatusEnum.Cancelled)
            {
                result.Success = false;
                result.Message = "订单已取消，不可下发";
                return result;
            }

            if ((OrderStatusEnum)order.OrderStatus == OrderStatusEnum.Dispatched)
            {
                result.Success = true;
                result.Message = "订单已下发，无需重复下发";
                return result;
            }

            var supplierCode = await _routeEngine.MatchByOrderAsync(order);
            if (string.IsNullOrEmpty(supplierCode))
            {
                await WriteLogAsync(order.Id, "", DispatchStatusEnum.Failed, attempt: 1, requestPayload: null,
                    responsePayload: null, statusCode: null, errorMessage: "未匹配到供应商（请配置路由规则或兜底规则）",
                    requestTime: DateTime.UtcNow, responseTime: DateTime.UtcNow);
                await uow.SaveChangesAsync();
                await uow.CompleteAsync();
                result.Success = false;
                result.Message = "未匹配到供应商";
                return result;
            }

            var supplierQuery = await _supplierRepo.GetQueryableAsync();
            var supplier = await supplierQuery.FirstOrDefaultAsync(x => x.Code == supplierCode);
            if (supplier is null)
            {
                await WriteLogAsync(order.Id, supplierCode!, DispatchStatusEnum.Failed, attempt: 1, requestPayload: null,
                    responsePayload: null, statusCode: null, errorMessage: $"供应商 {supplierCode} 不存在",
                    requestTime: DateTime.UtcNow, responseTime: DateTime.UtcNow);
                await uow.SaveChangesAsync();
                await uow.CompleteAsync();
                result.Success = false;
                result.Message = $"供应商 {supplierCode} 不存在";
                return result;
            }

            var attempt = await GetNextAttemptAsync(order.Id);

            // 加载扩展属性（仅在调用详情/下发时才查询扩展表）
            var extQuery = await _extensionRepo.GetQueryableAsync();
            var extension = await extQuery.FirstOrDefaultAsync(x => x.OrderId == order.Id);

            var payload = BuildPayload(order, extension?.AttributesJson);
            var payloadJson = JsonSerializer.Serialize(payload);
            var context = new SupplierContext
            {
                Supplier = MapSupplierInfo(supplier),
                Payload = payload
            };

            var requestTime = DateTime.UtcNow;
            var client = _clientFactory.Get((SupplierProtocolEnum)supplier.Protocol);
            var response = await client.SendAsync(context, default);
            var responseTime = DateTime.UtcNow;
            var status = response.Success ? DispatchStatusEnum.Success : DispatchStatusEnum.Failed;

            await WriteLogAsync(
                orderId: order.Id,
                supplierCode: supplier.Code,
                status: status,
                attempt: attempt,
                requestPayload: payloadJson,
                responsePayload: response.ResponseBody,
                statusCode: response.StatusCode,
                errorMessage: response.ErrorMessage,
                requestTime: requestTime,
                responseTime: responseTime);

            if (response.Success)
            {
                order.OrderStatus = (int)OrderStatusEnum.Dispatched;
                await _orderRepo.UpdateAsync(order);
                result.Success = true;
                result.SupplierCode = supplier.Code;
                result.Message = "下发成功";
            }
            else
            {
                result.Success = false;
                result.SupplierCode = supplier.Code;
                result.Message = response.ErrorMessage;
            }

            await uow.SaveChangesAsync();
            await uow.CompleteAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "订单 {OrderId} 下发执行异常", orderId);
            result.Success = false;
            result.Message = ex.Message;
            return result;
        }
    }

    private async Task<int> GetNextAttemptAsync(Guid orderId)
    {
        var query = await _logRepo.GetQueryableAsync();
        var latest = await query.Where(x => x.OrderId == orderId).OrderByDescending(x => x.CreationTime).FirstOrDefaultAsync();
        return latest is null ? 1 : latest.AttemptCount + 1;
    }

    private async Task WriteLogAsync(
        Guid orderId, string supplierCode, DispatchStatusEnum status, int attempt,
        string? requestPayload, string? responsePayload, int? statusCode, string? errorMessage,
        DateTime? requestTime, DateTime? responseTime)
    {
        if (errorMessage is not null && errorMessage.Length > 2000)
        {
            errorMessage = errorMessage[..2000];
        }

        var log = new DispatchLogEntity
        {
            OrderId = orderId,
            SupplierCode = supplierCode,
            Status = (int)status,
            AttemptCount = attempt,
            RequestPayload = requestPayload,
            ResponsePayload = responsePayload,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            RequestTime = requestTime,
            ResponseTime = responseTime,
            NextRetryTime = status == DispatchStatusEnum.Failed ? DateTime.UtcNow.AddSeconds(60) : null
        };
        await _logRepo.InsertAsync(log);
    }

    private static OrderDispatchPayload BuildPayload(OrderEntity order, string? attributesJson) => new()
    {
        OrderNo = order.OrderNo,
        ProductName = order.ProductName,
        BuyerId = order.BuyerId,
        OrderStatus = (OrderStatusEnum)order.OrderStatus,
        Industry = order.Industry,
        ProductCategory = order.ProductCategory,
        TotalAmount = order.TotalAmount,
        Remark = order.Remark,
        AttributesJson = attributesJson
    };

    private static SupplierInfo MapSupplierInfo(SupplierEntity supplier) => new()
    {
        Code = supplier.Code,
        Name = supplier.Name,
        ApiUrl = supplier.ApiUrl,
        AuthType = (AuthTypeEnum)supplier.AuthType,
        AuthConfig = supplier.AuthConfig,
        Protocol = (SupplierProtocolEnum)supplier.Protocol
    };
}