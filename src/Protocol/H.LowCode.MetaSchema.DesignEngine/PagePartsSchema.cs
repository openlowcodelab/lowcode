﻿using H.LowCode.MetaSchema;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class PagePartsSchema : PageSchemaBase
{
    [JsonPropertyName("comps")]
    public IList<ComponentPartsSchema> Components { get; set; } = [];
}
