using Volo.Abp.Domain.Entities.Auditing;

namespace H.SupplyChain.EntityFrameworkCore;

/// <summary>
/// 供应商定义
/// </summary>
public class SupplierEntity : FullAuditedEntity<Guid>
{
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
/// 接口定义（菜单接口、商品接口、下单接口等统一定义）
/// </summary>
public class ApiInterfaceEntity : FullAuditedEntity<Guid>
{
    /// <summary>接口编码（唯一，如 menu / product-detail / place-order）</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>接口名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>接口类型</summary>
    public int InterfaceType { get; set; }

    /// <summary>HTTP 方法</summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>接口路径</summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 供应商接口映射。
/// 基于接口定义，配置对应供应商的接口请求参数映射、返回值字段映射。
/// </summary>
public class SupplierInterfaceMappingEntity : FullAuditedEntity<Guid>
{
    /// <summary>供应商ID</summary>
    public Guid SupplierId { get; set; }

    /// <summary>接口定义ID</summary>
    public Guid InterfaceId { get; set; }

    /// <summary>供应商接口地址（覆盖供应商默认 ApiUrl，可空）</summary>
    public string? SupplierApiUrl { get; set; }

    /// <summary>请求参数映射（JSON，<see cref="List{FieldMapping}"/>）</summary>
    public string? RequestMappingJson { get; set; }

    /// <summary>返回值字段映射（JSON，<see cref="List{FieldMapping}"/>）</summary>
    public string? ResponseMappingJson { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }

    /// <summary>关联供应商</summary>
    public virtual SupplierEntity? Supplier { get; set; }

    /// <summary>关联接口定义</summary>
    public virtual ApiInterfaceEntity? Interface { get; set; }
}