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
}
