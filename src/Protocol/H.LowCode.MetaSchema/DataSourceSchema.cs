using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class DataSourceSchema : MetaSchemaBase
{
    [JsonPropertyName("aid")]
    public string AppId { get; set; }

    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("type")]
    public DataSourceTypeEnum DataSourceType { get; set; }

    public int PublishStatus { get; set; }

    #region 根据 DataSourceType 选用
    /// <summary>
    /// 数据表数据源
    /// </summary>
    [JsonPropertyName("fields")]
    public List<TableFieldSchema> TableFields { get; set; } = [];

    /// <summary>
    /// API 数据源
    /// </summary>
    [JsonPropertyName("api")]
    public APISchema API { get; set; }

    /// <summary>
    /// 选项数据源
    /// </summary>
    [JsonPropertyName("ops")]
    public List<OptionSchema> Options { get; set; } = [];

    /// <summary>
    /// 
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 字典数据源
    /// </summary>
    [JsonPropertyName("vals")]
    public IDictionary<string, string> Values { get; set; }
    #endregion
}