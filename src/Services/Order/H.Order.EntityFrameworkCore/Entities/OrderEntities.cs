using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Order.EntityFrameworkCore;

/// <summary>
/// 订单实体（核心表，仅行业无关的最小属性集）
/// </summary>
public class OrderEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>订单号</summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>商品名称</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>买家ID</summary>
    public string BuyerId { get; set; } = string.Empty;

    /// <summary>订单状态</summary>
    public int OrderStatus { get; set; }

    /// <summary>行业（自由字符串）</summary>
    public string? Industry { get; set; }

    /// <summary>商品类别（自由字符串）</summary>
    public string? ProductCategory { get; set; }

    /// <summary>总金额</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 订单扩展实体（按行业存储特有属性，JSON 格式）
/// </summary>
public class OrderExtensionEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>订单ID（一对一）</summary>
    public Guid OrderId { get; set; }

    /// <summary>行业特有属性 JSON</summary>
    public string? AttributesJson { get; set; }

    /// <summary>关联订单</summary>
    public virtual OrderEntity? Order { get; set; }
}

/// <summary>
/// 供应商定义
/// </summary>
public class SupplierEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>供应商编码（唯一）</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>供应商名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>显示名称</summary>
    public string? DisplayName { get; set; }

    /// <summary>API 地址</summary>
    public string? ApiUrl { get; set; }

    /// <summary>认证方式</summary>
    public int AuthType { get; set; }

    /// <summary>认证配置（JSON）</summary>
    public string? AuthConfig { get; set; }

    /// <summary>对接协议</summary>
    public int Protocol { get; set; }

    /// <summary>协议配置（JSON）</summary>
    public string? ProtocolConfig { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 路由规则
/// </summary>
public class RouteRuleEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>规则名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>命中的供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>规则类型</summary>
    public int RuleType { get; set; }

    /// <summary>优先级</summary>
    public int Priority { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>条件集合 JSON</summary>
    public string? ConditionsJson { get; set; }

    /// <summary>是否兜底</summary>
    public bool Fallback { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 下发日志
/// </summary>
public class DispatchLogEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>订单ID</summary>
    public Guid OrderId { get; set; }

    /// <summary>供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>下发状态</summary>
    public int Status { get; set; }

    /// <summary>尝试次数</summary>
    public int AttemptCount { get; set; }

    /// <summary>请求负载 JSON</summary>
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