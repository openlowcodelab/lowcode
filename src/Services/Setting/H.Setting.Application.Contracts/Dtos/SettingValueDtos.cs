using Volo.Abp.Application.Dtos;

namespace H.Setting.Application.Contracts;

/// <summary>
/// 配置项 DTO（对应 ABP SettingManagement 的设置值）
/// </summary>
public class SettingValueDto : FullAuditedEntityDto<Guid>
{
    /// <summary>配置名称（关联配置定义的 Name）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>配置值</summary>
    public string? Value { get; set; }

    /// <summary>提供者名称（G=全局, T=租户, U=用户）</summary>
    public string ProviderName { get; set; } = SettingValueProviders.Global;

    /// <summary>提供者键（如租户Id/用户Id，全局为空）</summary>
    public string? ProviderKey { get; set; }

    /// <summary>关联配置定义的显示名称（冗余，便于列表展示）</summary>
    public string? DisplayName { get; set; }
}

/// <summary>
/// 新增/修改配置项 DTO
/// </summary>
public class CreateUpdateSettingValueDto
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string ProviderName { get; set; } = SettingValueProviders.Global;
    public string? ProviderKey { get; set; }
}

/// <summary>
/// 配置项查询参数
/// </summary>
public class SettingValueQueryDto : PagedResultRequestDto
{
    /// <summary>按配置名称模糊过滤</summary>
    public string? Filter { get; set; }

    /// <summary>按提供者名称过滤</summary>
    public string? ProviderName { get; set; }
}

/// <summary>
/// 配置定义下拉项（用于配置项页面选择关联的定义）
/// </summary>
public class SettingDefinitionLookupDto
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
}

/// <summary>
/// 常用配置项提供者名称
/// </summary>
public static class SettingValueProviders
{
    /// <summary>全局</summary>
    public const string Global = "G";

    /// <summary>租户</summary>
    public const string Tenant = "T";

    /// <summary>用户</summary>
    public const string User = "U";
}
