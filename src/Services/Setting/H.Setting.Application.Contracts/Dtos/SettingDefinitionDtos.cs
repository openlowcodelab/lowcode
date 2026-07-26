using H.Abstractions;

namespace H.Setting.Application.Contracts;

/// <summary>
/// 配置定义 DTO（对应 ABP SettingManagement 的设置定义）
/// </summary>
public class SettingDefinitionDto : FullAuditedEntityDto<Guid>
{
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
    public bool IsInherited { get; set; }

    /// <summary>是否加密存储</summary>
    public bool IsEncrypted { get; set; }
}

/// <summary>
/// 新增/修改配置定义 DTO
/// </summary>
public class CreateUpdateSettingDefinitionDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DefaultValue { get; set; }
    public bool IsVisibleToClients { get; set; }
    public string? Providers { get; set; }
    public bool IsInherited { get; set; } = true;
    public bool IsEncrypted { get; set; }
}

/// <summary>
/// 配置定义查询参数
/// </summary>
public class SettingDefinitionQueryDto : PagedResultRequestDto
{
    /// <summary>按名称/显示名称模糊过滤</summary>
    public string? Filter { get; set; }
}
