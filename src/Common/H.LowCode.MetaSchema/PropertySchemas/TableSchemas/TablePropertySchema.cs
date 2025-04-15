using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class TablePropertySchema
{
    [JsonPropertyName("tcols")]
    public IList<TableColumnSchema> Columns { get; set; } = [];

    [JsonPropertyName("searchs")]
    public IList<TableSearchItemSchema> SearchItems { get; set; } = [];

    [JsonPropertyName("btns")]
    public IList<TableButtonSchema> Buttons { get; set; } = [];
}
