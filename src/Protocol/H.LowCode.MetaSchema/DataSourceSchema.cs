using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class DataSourceSchema
{
    public string Id { get; set; }

    public string Key { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }

    [JsonPropertyName("type")]
    public DataSourceEnum DataSourceType { get; set; }

    public string ModifiedUser { get; set; }

    public DateTime ModifiedTime { get; set; }

    public string PublishStatus { get; set; }

    #region 根据 DataSourceType 选用
    /// <summary>
    /// 数据表数据源
    /// </summary>
    [JsonPropertyName("fields")]
    public List<DataSourceTableFieldSchema> DataSourceTableFields { get; set; } = [];

    /// <summary>
    /// API 数据源
    /// </summary>
    [JsonPropertyName("api")]
    public DataSourceAPISchema DataSourceAPI { get; set; }

    /// <summary>
    /// 选项数据源
    /// </summary>
    [JsonPropertyName("options")]
    public List<DataSourceOptionSchema> DataSourceOptions { get; set; } = [];

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

public enum DataSourceEnum
{
    Default,
    API,
    Options,
    SQL,
    Expression, //表达式
    Fiexd  //固定值
}

public class DataSourceTableFieldSchema
{
    public string FieldKey { get; set; }

    public string FieldName { get; set; }

    public string FieldType { get; set; }

    public bool IsPrimaryKey { get; set; }

    public bool IsRequired { get; set; }

    public bool IsUnique { get; set; }
}

public class DataSourceAPISchema
{
    public string BaseUrl { get; set; }

    public string Path { get; set; }

    public string Method { get; set; }

    public IDictionary<string, string> Headers { get; set; }

    public IDictionary<string, string> Queries { get; set; }

    public string Body { get; set; }

    public string Description { get; set; }
}

public class DataSourceOptionSchema
{
    public string OptionKey { get; set; }

    public string OptionName { get; set; }
}
