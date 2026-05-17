using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.SystemManagement.Application.Contracts.Dtos;

/// <summary>
/// 设置项DTO
/// </summary>
public class SettingItemDto
{
    /// <summary>
    /// 设置名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 设置值
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string? GroupName { get; set; }
}

/// <summary>
/// 更新设置项DTO
/// </summary>
public class UpdateSettingItemDto
{
    /// <summary>
    /// 设置名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 设置值
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// 设置分组DTO
/// </summary>
public class SettingGroupDto
{
    /// <summary>
    /// 分组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 分组显示名称
    /// </summary>
    public string GroupDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 设置项列表
    /// </summary>
    public List<SettingItemDto> Items { get; set; } = new();
}
