using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public class APISchema
{
    [JsonPropertyName("burl")]
    public string BaseUrl { get; set; }

    [JsonPropertyName("p")]
    public string Path { get; set; }

    [JsonPropertyName("mth")]
    public string Method { get; set; }

    [JsonPropertyName("prs")]
    public IList<APIQuerySchema> Queries { get; set; } = [];

    [JsonPropertyName("bd")]
    public APIBodySchema Body { get; set; }

    [JsonPropertyName("hds")]
    public IList<APIHeaderSchema> Headers { get; set; } = [];
}

public class APIQuerySchema
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("t")]
    public string Type { get; set; }

    [JsonPropertyName("val")]
    public string Value { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set;}
}

public class APIBodySchema
{
    public string Type { get; set; }

    public string Value { get; set; }
}

public class APIHeaderSchema
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("t")]
    public string Type { get; set; }

    [JsonPropertyName("val")]
    public string Value { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }
}