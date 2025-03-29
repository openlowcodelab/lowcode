using H.LowCode.MetaSchema;
using System;
using System.Text.Json.Serialization;

namespace H.LowCode.DesignEngine.Model;

public class AppListModel
{
    public string Id { get; set; }

    [JsonPropertyName("url")]
    public string SiteUrl { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    public string Icon { get; set; }

    [JsonPropertyName("pic")]
    public string Picture { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }

    [JsonPropertyName("pub")]
    public PublishStatusEnum PublishStatus { get; set; }

    [JsonPropertyName("platform")]
    public SupportPlatformEnum[] SupportPlatforms { get; set; } = [0];
}
