using H.LowCode.MetaSchema;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class PagePartsSchema : PageSchema
{
    [JsonPropertyName("comps")]
    public new IList<ComponentPartsSchema> Components { get; set; } = [];
}
