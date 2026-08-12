using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class AppPartsSchema : AppSchemaBase
{
    /// <summary>
    /// 默认首页（页面Id）
    /// </summary>
    [JsonPropertyName("home")]
    public string? HomePageId { get; set; }

    /// <summary>
    /// 主题主色（十六进制色值，如 #165DFF）
    /// </summary>
    [JsonPropertyName("theme")]
    public string? ThemeColor { get; set; }

    /// <summary>
    /// 访问模式
    /// </summary>
    [JsonPropertyName("access")]
    public AppAccessModeEnum AccessMode { get; set; } = AppAccessModeEnum.LoginRequired;

    /// <summary>
    /// 备注
    /// </summary>
    [JsonPropertyName("remark")]
    public string? Remark { get; set; }
}

/// <summary>
/// 应用访问模式
/// </summary>
public enum AppAccessModeEnum
{
    /// <summary>
    /// 公开访问（无需登录）
    /// </summary>
    Public = 0,

    /// <summary>
    /// 登录后可访问
    /// </summary>
    LoginRequired = 1,

    /// <summary>
    /// 仅应用成员可访问
    /// </summary>
    MemberOnly = 2
}
