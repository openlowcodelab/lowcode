using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

/// <summary>
/// List 循环数据源配置
/// </summary>
public class ListDataSourceSchema
{
    /// <summary>
    /// 固定数据源（用于设计时预览）
    /// </summary>
    [JsonPropertyName("fxdata")]
    public IList<Dictionary<string, object>>? FixedData { get; set; }

    /// <summary>
    /// API 数据源配置
    /// </summary>
    [JsonPropertyName("apids")]
    public APIDataSourceSchema? APIDataSource { get; set; }

    /// <summary>
    /// SQL 数据源配置
    /// </summary>
    [JsonPropertyName("sqlds")]
    public SQLDataSourceSchema? SQLDataSource { get; set; }

    /// <summary>
    /// 数据响应路径（用于提取数组数据，如 "data.list"）
    /// </summary>
    [JsonPropertyName("datapath")]
    public string? DataPath { get; set; }

    /// <summary>
    /// 排序字段
    /// </summary>
    [JsonPropertyName("orderby")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// 是否倒序
    /// </summary>
    [JsonPropertyName("orderdesc")]
    public bool OrderDesc { get; set; }
}
