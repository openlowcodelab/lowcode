using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public abstract class PartsMetaSchemaBase
{
    [JsonPropertyName("cu")]
    public string CreatedUser { get; set; }

    [JsonPropertyName("ct")]
    public DateTime CreatedTime { get; set; }

    [JsonPropertyName("mu")]
    public string? ModifiedUser { get; set; }

    [JsonPropertyName("mt")]
    public DateTime? ModifiedTime { get; set; }
}
