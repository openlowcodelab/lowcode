using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.SupplyChain.EntityFrameworkCore;

/// <summary>
/// 供应商定义
/// </summary>
public class SupplierEntity : AuditedEntity<string>, IMultiTenant
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
