using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class DataSourceSchema : MetaSchemaBase
{
    [JsonPropertyName("aid")]
    public string AppId { get; set; }

    public string Id { get; set; } = ShortIdGenerator.Generate();

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

    [JsonPropertyName("pub")]
    public bool PublishStatus { get; set; }

    #region DataSourceType=Table
    /// <summary>
    /// 数据表数据源
    /// </summary>
    [JsonPropertyName("fields")]
    public IList<TableFieldSchema> TableFields { get; set; } = [];

    /// <summary>
    /// 是否启用软删除
    /// </summary>
    public bool EnableSoftDelete { get; set; }
    #endregion

    #region DataSourceType=API
    /// <summary>
    /// API 数据源
    /// </summary>
    [JsonPropertyName("api")]
    public APISchema API { get; set; }
    #endregion

    #region DataSourceType=Option
    /// <summary>
    /// 选项数据源
    /// </summary>
    [JsonPropertyName("ops")]
    public OptionSchema[] Options { get; set; }

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