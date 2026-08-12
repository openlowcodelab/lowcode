using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentAttributeDefineGroupSchema
{
    [JsonPropertyName("attrdefs")]
    public ComponentAttributeDefineSchema[] AttributeDefines { get; set; }
}
