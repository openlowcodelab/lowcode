using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Setting.EntityFrameworkCore;

/// <summary>
/// 配置定义实体（结构参考 ABP SettingManagement 的 SettingDefinitionRecord）
/// </summary>
public class SettingDefinition : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>配置名称（唯一标识）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string? Description { get; set; }

    /// <summary>默认值</summary>
    public string? DefaultValue { get; set; }

    /// <summary>是否对客户端可见</summary>
    public bool IsVisibleToClients { get; set; }

    /// <summary>允许的提供者（逗号分隔，如 G,T,U）</summary>
    public string? Providers { get; set; }

    /// <summary>是否可被下层提供者继承</summary>
    public bool IsInherited { get; set; } = true;

    /// <summary>是否加密存储</summary>
    public bool IsEncrypted { get; set; }
}
