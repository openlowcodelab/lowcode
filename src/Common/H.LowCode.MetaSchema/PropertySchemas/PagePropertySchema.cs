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
    public string TitleWidth { get; set; }

    [JsonPropertyName("ds")]
    public PageDataSourceSchema DataSource { get; set; } = new();
}
