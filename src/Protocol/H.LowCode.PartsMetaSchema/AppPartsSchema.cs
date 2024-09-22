using H.LowCode.MetaSchema;
using System.Text.Json.Serialization;

namespace H.LowCode.PartsMetaSchema;

public class AppPartsSchema : BasePartsMetaSchema
{
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("pic")]
    public string Picture { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }

    [JsonPropertyName("v")]
    public string Version { get; set; }

    [JsonPropertyName("state")]
    public PublishStateEnum PublishState { get; set; }

    [JsonPropertyName("platform")]
    public SupportPlatformEnum[] SupportPlatforms { get; set; } = [0];
}
