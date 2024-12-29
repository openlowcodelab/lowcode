using H.LowCode.MetaSchema;
using System.Text.Json.Serialization;

namespace H.LowCode.PartsMetaSchema;

public class PagePartsSchema : PageSchema
{
    [JsonPropertyName("comps")]
    public new IList<ComponentPartsSchema> Components { get; set; } = [];
}
