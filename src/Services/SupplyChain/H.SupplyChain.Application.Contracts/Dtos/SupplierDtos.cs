using H.Abstractions;

namespace H.SupplyChain.Application.Contracts;

/// <summary>
/// 供应商 DTO
/// </summary>
public class SupplierDto : FullAuditedEntityDto<Guid>
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
    public AuthTypeEnum AuthType { get; set; }

    /// <summary>认证配置（JSON）</summary>
    public string? AuthConfig { get; set; }

    /// <summary>对接协议</summary>
    public SupplierProtocolEnum Protocol { get; set; }

    /// <summary>协议配置（JSON）</summary>
    public string? ProtocolConfig { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 创建供应商 DTO
/// </summary>
public class CreateSupplierDto
{
    /// <summary>供应商编码</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>供应商名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>显示名称</summary>
    public string? DisplayName { get; set; }

    /// <summary>API 地址</summary>
    public string? ApiUrl { get; set; }

    /// <summary>认证方式</summary>
    public AuthTypeEnum AuthType { get; set; } = AuthTypeEnum.None;

    /// <summary>认证配置（JSON）</summary>
    public string? AuthConfig { get; set; }

    /// <summary>对接协议</summary>
    public SupplierProtocolEnum Protocol { get; set; } = SupplierProtocolEnum.Http;

    /// <summary>协议配置（JSON）</summary>
    public string? ProtocolConfig { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新供应商 DTO
/// </summary>
public class UpdateSupplierDto
{
    /// <summary>供应商名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>显示名称</summary>
    public string? DisplayName { get; set; }

    /// <summary>API 地址</summary>
    public string? ApiUrl { get; set; }

    /// <summary>认证方式</summary>
    public AuthTypeEnum AuthType { get; set; }

    /// <summary>认证配置（JSON）</summary>
    public string? AuthConfig { get; set; }

    /// <summary>对接协议</summary>
    public SupplierProtocolEnum Protocol { get; set; }

    /// <summary>协议配置（JSON）</summary>
    public string? ProtocolConfig { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 供应商查询参数
/// </summary>
public class SupplierQueryDto : PagedResultRequestDto
{
    /// <summary>关键词（编码或名称）</summary>
    public string? Filter { get; set; }

    /// <summary>是否启用</summary>
    public bool? IsEnabled { get; set; }
}