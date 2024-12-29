using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class AppSchema : MetaSchemaBase
{
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    public string Icon { get; set; }

    [JsonPropertyName("pic")]
    public string Picture { get; set; }

    [JsonPropertyName("desc")]
    public string Description {  get; set; }

    [JsonPropertyName("v")]
    public string Version { get; set; }

    [JsonPropertyName("pub")]
    public PublishStatusEnum PublishStatus { get; set; }

    [JsonPropertyName("platform")]
    public SupportPlatformEnum[] SupportPlatforms { get; set; } = [0];
}
