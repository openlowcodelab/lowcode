using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

/// <summary>
/// 应用模板物料（某个应用的完整快照）
/// </summary>
public class AppTemplateSchema : PartsMetaSchemaBase
{
    /// <summary>
    /// 应用模板Id
    /// </summary>
    [JsonPropertyName("tid")]
    public required string TemplateId { get; set; }

    [JsonPropertyName("n")]
    public string? Name { get; set; }

    [JsonPropertyName("desc")]
    public string? Description { get; set; }

    public string? Icon { get; set; }

    [JsonPropertyName("theme")]
    public string? ThemeColor { get; set; }

    [JsonPropertyName("platform")]
    public SupportPlatformEnum[] SupportPlatforms { get; set; } = [0];

    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }

    #region 快照内容
    /// <summary>
    /// 应用元信息快照
    /// </summary>
    [JsonPropertyName("app")]
    public AppPartsSchema? App { get; set; }

    /// <summary>
    /// 页面快照
    /// </summary>
    [JsonPropertyName("pages")]
    public List<PagePartsSchema> Pages { get; set; } = [];

    /// <summary>
    /// 菜单快照（扁平列表）
    /// </summary>
    [JsonPropertyName("menus")]
    public List<MenuSchema> Menus { get; set; } = [];

    /// <summary>
    /// 数据源快照
    /// </summary>
    [JsonPropertyName("datasources")]
    public List<DataSourceSchema> DataSources { get; set; } = [];
    #endregion
}
