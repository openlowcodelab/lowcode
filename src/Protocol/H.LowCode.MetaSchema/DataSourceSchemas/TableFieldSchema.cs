using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public record TableFieldSchema
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    [JsonPropertyName("t")]
    public string Type { get; set; }

    [JsonPropertyName("pk")]
    public bool IsPrimaryKey { get; set; }

    [JsonPropertyName("required")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("unique")]
    public bool IsUnique { get; set; }

    [JsonPropertyName("m")]
    public string Comments { get; set; }
}
