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

    public int Order { get; set; }

    [JsonPropertyName("type")]
    public DataSourceTypeEnum DataSourceType { get; set; }

    public int PublishStatus { get; set; }

    public string ModifiedUser { get; set; }

    public DateTime ModifiedTime { get; set; }
}
