using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class PageSchema : PageSchemaBase
{
    [JsonPropertyName("comps")]
    public new IList<ComponentSchema> Components { get; set; } = [];
}