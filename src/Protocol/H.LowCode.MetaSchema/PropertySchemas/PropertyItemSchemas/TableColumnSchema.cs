using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class TableColumnSchema
{
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("t")]
    public string Title { get; set; }

    [JsonPropertyName("pk")]
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// 列顺序
    /// </summary>
    [JsonPropertyName("")]
    public int Order { get; set; }

    /// <summary>
    /// 是否支持过滤
    /// </summary>
    public bool Filterable { get; set; }

    /// <summary>
    /// 是否支持排序
    /// </summary>
    public bool Sortable { get; set; }
}
