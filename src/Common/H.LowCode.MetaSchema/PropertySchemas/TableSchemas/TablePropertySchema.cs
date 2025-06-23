using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class TablePropertySchema
{
    [JsonPropertyName("tcols")]
    public IList<TableColumnSchema> Columns { get; set; } = [];

    [JsonPropertyName("searchs")]
    public IList<TableSearchItemSchema> SearchItems { get; set; } = [];

    /// <summary>
    /// 列表上方按钮
    /// </summary>
    [JsonPropertyName("tbtns")]
    public IList<TableButtonSchema> TopButtons { get; set; } = [];

    /// <summary>
    /// 表格行按钮
    /// </summary>
    [JsonPropertyName("rbtns")]
    public IList<TableButtonSchema> RowButtons { get; set; } = [];
}
