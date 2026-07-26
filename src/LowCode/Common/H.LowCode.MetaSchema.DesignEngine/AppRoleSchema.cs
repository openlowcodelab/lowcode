using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

/// <summary>
/// 应用角色
/// </summary>
public class AppRoleSchema
{
    [JsonPropertyName("aid")]
    public string AppId { get; set; }

    /// <summary>
    /// 角色Key（应用内唯一）
    /// </summary>
    [JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }

    /// <summary>
    /// 权限项集合，如 page.view / data.read / data.write / app.manage
    /// </summary>
    [JsonPropertyName("perms")]
    public string[] Permissions { get; set; } = [];

    /// <summary>
    /// 是否内置角色（内置角色不可删除）
    /// </summary>
    [JsonPropertyName("builtin")]
    public bool IsBuiltin { get; set; }
}
