using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsAttributeDefineGroupSchema
{
    [JsonPropertyName("gn")]
    public string? GroupName { get; set; }

    [JsonPropertyName("attrdefs")]
    public ComponentPartsAttributeDefineSchema[] AttributeDefines { get; set; } = [];
}
