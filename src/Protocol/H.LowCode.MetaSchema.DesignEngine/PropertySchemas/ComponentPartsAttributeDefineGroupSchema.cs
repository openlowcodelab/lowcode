using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsAttributeDefineGroupSchema
{
    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    [JsonPropertyName("attrs")]
    public IList<ComponentPartsAttributeDefineSchema> AttributeDefines { get; set; } = [];
}
