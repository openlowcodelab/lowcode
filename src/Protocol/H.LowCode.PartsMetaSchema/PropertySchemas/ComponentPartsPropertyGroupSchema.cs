using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.PartsMetaSchema;

public class ComponentPartsPropertyGroupSchema : ComponentPropertyGroupSchema
{
    [JsonPropertyName("props")]
    public new IList<ComponentPartsPropertySchema> Properties { get; set; } = [];
}
