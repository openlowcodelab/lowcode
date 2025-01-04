using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public class OptionSchema
{
    [JsonPropertyName("k")]
    public string Key { get; set; }

    [JsonPropertyName("v")]
    public string Value { get; set; }
}
