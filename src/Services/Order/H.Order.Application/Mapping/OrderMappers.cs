using H.Order.Application.Contracts;
using H.Order.EntityFrameworkCore;

namespace H.Order.Application.Mapping;

/// <summary>
/// 实体 <-> DTO 手工映射（与 Approval 服务保持一致的轻量映射风格，
/// 避免依赖 ABP 的 AutoObjectMappingProvider）。
/// </summary>
public static class OrderMappers
{
    // === Order ===
    public static OrderDto ToDto(this OrderEntity e) => new()
    {
        Id = e.Id,
        OrderNo = e.OrderNo,
        ProductName = e.ProductName,
        BuyerId = e.BuyerId,
        OrderStatus = (OrderStatusEnum)e.OrderStatus,
        Industry = e.Industry,
        ProductCategory = e.ProductCategory,
        TotalAmount = e.TotalAmount,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static OrderDetailDto ToDetailDto(this OrderEntity e) => new()
    {
        Id = e.Id,
        OrderNo = e.OrderNo,
        ProductName = e.ProductName,
        BuyerId = e.BuyerId,
        OrderStatus = (OrderStatusEnum)e.OrderStatus,
        Industry = e.Industry,
        ProductCategory = e.ProductCategory,
        TotalAmount = e.TotalAmount,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    /// <summary>用创建 DTO 构造一个新实体（不含 Id，由 ABP 仓库赋值）</summary>
    public static OrderEntity ToEntity(this CreateOrderDto input) => new()
    {
        ProductName = input.ProductName,
        BuyerId = input.BuyerId,
        OrderStatus = (int)input.OrderStatus,
        Industry = input.Industry,
        ProductCategory = input.ProductCategory,
        TotalAmount = input.TotalAmount,
        Remark = input.Remark
    };

    /// <summary>把更新 DTO 应用到已加载的实体</summary>
    public static void Apply(this UpdateOrderDto input, OrderEntity entity)
    {
        entity.ProductName = input.ProductName;
        entity.BuyerId = input.BuyerId;
        entity.OrderStatus = (int)input.OrderStatus;
        entity.Industry = input.Industry;
        entity.ProductCategory = input.ProductCategory;
        entity.TotalAmount = input.TotalAmount;
        entity.Remark = input.Remark;
    }

    // === Supplier ===
    public static SupplierDto ToDto(this SupplierEntity e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Name = e.Name,
        DisplayName = e.DisplayName,
        ApiUrl = e.ApiUrl,
        AuthType = (AuthTypeEnum)e.AuthType,
        AuthConfig = e.AuthConfig,
        Protocol = (SupplierProtocolEnum)e.Protocol,
        ProtocolConfig = e.ProtocolConfig,
        IsEnabled = e.IsEnabled,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static SupplierEntity ToEntity(this CreateSupplierDto input) => new()
    {
        Code = input.Code,
        Name = input.Name,
        DisplayName = input.DisplayName,
        ApiUrl = input.ApiUrl,
        AuthType = (int)input.AuthType,
        AuthConfig = input.AuthConfig,
        Protocol = (int)input.Protocol,
        ProtocolConfig = input.ProtocolConfig,
        IsEnabled = input.IsEnabled,
        Remark = input.Remark
    };

    public static void Apply(this UpdateSupplierDto input, SupplierEntity entity)
    {
        entity.Name = input.Name;
        entity.DisplayName = input.DisplayName;
        entity.ApiUrl = input.ApiUrl;
        entity.AuthType = (int)input.AuthType;
        entity.AuthConfig = input.AuthConfig;
        entity.Protocol = (int)input.Protocol;
        entity.ProtocolConfig = input.ProtocolConfig;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
    }

    // === RouteRule ===
    public static RouteRuleDto ToDto(this RouteRuleEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        SupplierCode = e.SupplierCode,
        RuleType = (RouteRuleTypeEnum)e.RuleType,
        Priority = e.Priority,
        IsEnabled = e.IsEnabled,
        ConditionsJson = e.ConditionsJson,
        Fallback = e.Fallback,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static RouteRuleEntity ToEntity(this CreateRouteRuleDto input) => new()
    {
        Name = input.Name,
        SupplierCode = input.SupplierCode,
        RuleType = (int)input.RuleType,
        Priority = input.Priority,
        IsEnabled = input.IsEnabled,
        ConditionsJson = input.ConditionsJson,
        Fallback = input.Fallback,
        Remark = input.Remark
    };

    public static void Apply(this UpdateRouteRuleDto input, RouteRuleEntity entity)
    {
        entity.Name = input.Name;
        entity.SupplierCode = input.SupplierCode;
        entity.RuleType = (int)input.RuleType;
        entity.Priority = input.Priority;
        entity.IsEnabled = input.IsEnabled;
        entity.ConditionsJson = input.ConditionsJson;
        entity.Fallback = input.Fallback;
        entity.Remark = input.Remark;
    }

    // === DispatchLog ===
    public static DispatchLogDto ToDto(this DispatchLogEntity e) => new()
    {
        Id = e.Id,
        OrderId = e.OrderId,
        SupplierCode = e.SupplierCode,
        Status = (DispatchStatusEnum)e.Status,
        AttemptCount = e.AttemptCount,
        RequestPayload = e.RequestPayload,
        ResponsePayload = e.ResponsePayload,
        StatusCode = e.StatusCode,
        ErrorMessage = e.ErrorMessage,
        NextRetryTime = e.NextRetryTime,
        RequestTime = e.RequestTime,
        ResponseTime = e.ResponseTime,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };
}