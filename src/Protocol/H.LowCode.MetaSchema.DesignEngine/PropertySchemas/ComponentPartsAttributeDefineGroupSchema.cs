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
    [JsonPropertyName("gn")]
    public string GroupName { get; set; }

    [JsonPropertyName("attrdefs")]
    public ComponentPartsAttributeDefineSchema[] AttributeDefines { get; set; } = [];
}
