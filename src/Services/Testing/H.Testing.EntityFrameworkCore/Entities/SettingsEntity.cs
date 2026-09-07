using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 测试模块全局设置（单条记录）
/// </summary>
public class SettingsEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public string Key { get; set; }

    public string? Value { get; set; }

    /// <summary>提供者名称（G=全局, T=租户, U=用户）</summary>
    public string ProviderName { get; set; } = SettingProviders.Global;

    /// <summary>提供者键（用户级为用户Id，全局为空）</summary>
    public string? ProviderKey { get; set; }
}
