using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

/// <summary>
/// 应用成员
/// </summary>
public class AppMemberSchema
{
    [JsonPropertyName("aid")]
    public string AppId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = ShortIdGenerator.Generate();

    /// <summary>
    /// 用户Id
    /// </summary>
    [JsonPropertyName("uid")]
    public string UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [JsonPropertyName("un")]
    public string UserName { get; set; }

    /// <summary>
    /// 角色Key
    /// </summary>
    [JsonPropertyName("rk")]
    public string RoleKey { get; set; }

    /// <summary>
    /// 加入时间
    /// </summary>
    [JsonPropertyName("jt")]
    public DateTime JoinTime { get; set; } = DateTime.UtcNow;
}
