using H.Util.Ids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public record OptionDataSourceSchema
{
    [JsonIgnore]
    public string Id { get; set; } = ShortIdGenerator.Generate();

    [JsonPropertyName("l")]
    public string Label { get; set; }

    [JsonPropertyName("v")]
    public string Value { get; set; }
}
