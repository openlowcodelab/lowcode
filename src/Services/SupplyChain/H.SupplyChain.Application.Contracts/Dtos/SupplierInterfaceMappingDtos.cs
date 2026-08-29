using H.Abp.Application.Contracts;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 供应商接口映射 DTO。
/// 基于接口定义，配置对应供应商的请求参数映射与返回值字段映射。
/// </summary>
public class SupplierInterfaceMappingDto : AuditedEntityDto<long>
{
    /// <summary>供应商ID</summary>
    public string SupplierId { get; set; }

    /// <summary>供应商编码</summary>
    public string SupplierCode { get; set; } = string.Empty;

    /// <summary>接口定义ID</summary>
    public long InterfaceId { get; set; }

    /// <summary>接口编码（冗余便于展示）</summary>
    public string InterfaceCode { get; set; } = string.Empty;

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
}

/// <summary>
/// 创建供应商接口映射 DTO
/// </summary>
public class CreateSupplierInterfaceMappingDto
{
    /// <summary>供应商ID</summary>
    public required string SupplierId { get; set; }

    /// <summary>接口定义ID</summary>
    public long InterfaceId { get; set; }

    /// <summary>供应商接口地址</summary>
    public string? SupplierApiUrl { get; set; }

    /// <summary>请求参数映射（JSON）</summary>
    public string? RequestMappingJson { get; set; }

    /// <summary>返回值字段映射（JSON）</summary>
    public string? ResponseMappingJson { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新供应商接口映射 DTO
/// </summary>
public class UpdateSupplierInterfaceMappingDto
{
    /// <summary>供应商接口地址</summary>
    public string? SupplierApiUrl { get; set; }

    /// <summary>请求参数映射（JSON）</summary>
    public string? RequestMappingJson { get; set; }

    /// <summary>返回值字段映射（JSON）</summary>
    public string? ResponseMappingJson { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 供应商接口映射查询参数
/// </summary>
public class SupplierInterfaceMappingQueryDto : PagedResultRequestDto
{
    /// <summary>供应商ID</summary>
    public string? SupplierId { get; set; }

    /// <summary>接口定义ID</summary>
    public long? InterfaceId { get; set; }

    /// <summary>是否启用</summary>
    public bool? IsEnabled { get; set; }
}