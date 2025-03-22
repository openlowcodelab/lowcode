using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class PageSchema : PageSchemaBase
{
    [JsonPropertyName("comps")]
    public IList<ComponentSchema> Components { get; set; } = [];
}