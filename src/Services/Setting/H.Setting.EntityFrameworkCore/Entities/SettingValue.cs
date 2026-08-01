using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Setting.EntityFrameworkCore;

/// <summary>
/// 配置项（配置值）实体（结构参考 ABP SettingManagement 的 Setting）
/// </summary>
public class SettingValue : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>配置名称（关联配置定义的 Name）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>配置值</summary>
    public string? Value { get; set; }

    /// <summary>提供者名称（G=全局, T=租户, U=用户）</summary>
    public string ProviderName { get; set; } = "G";

    /// <summary>提供者键（如租户Id/用户Id，全局为空）</summary>
    public string? ProviderKey { get; set; }
}
