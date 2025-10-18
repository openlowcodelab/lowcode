using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class PagePropertySchema
{
    /// <summary>
    /// 页面布局（1:一列、2:二列、3:三列、4:四列）
    /// </summary>
    [JsonPropertyName("playout")]
    public int PageLayout { get; set; } = 2;

    /// <summary>
    /// 标题宽度
    /// </summary>
    [JsonPropertyName("titlew")]
    public string? TitleWidth { get; set; }

    /// <summary>
    /// 默认样式
    /// </summary>
    [JsonPropertyName("dsty")]
    public string? DefaultStyle { get; set; }

    /// <summary>
    /// 自定义样式
    /// </summary>
    [JsonPropertyName("csty")]
    public string? CustomStyle { get; set; }

    /// <summary>
    /// 页面数据源
    /// </summary>
    [JsonPropertyName("ds")]
    public PageDataSourceSchema? DataSource { get; set; } = new();
}
