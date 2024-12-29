using H.LowCode.MetaSchema;
using System;
using System.Text.Json.Serialization;

namespace H.LowCode.DesignEngine.Model;

public class DataSourceListModel
{
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 额外信息
    /// </summary>
    [JsonPropertyName("extra")]
    public string Extra { get; set; }

    public int Order { get; set; }

    [JsonPropertyName("type")]
    public DataSourceTypeEnum DataSourceType { get; set; }

    [JsonPropertyName("pub")]
    public bool PublishStatus { get; set; }

    public string ModifiedUser { get; set; }

    public DateTime ModifiedTime { get; set; }
}
