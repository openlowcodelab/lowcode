using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class PageDataSourceSchema
{
    [JsonPropertyName("dst")]
    public PageDataSourceTypeEnum DataSourceType { get; set; }

    [JsonPropertyName("dsid")]
    public string? DataSourceId { get; set; }

    [JsonPropertyName("dsn")]
    public string? DataSourceName { get; set; }

    [JsonPropertyName("dsv")]
    public string? DataSourceValue { get; set; }
}