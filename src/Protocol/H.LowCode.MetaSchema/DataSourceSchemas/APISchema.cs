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

    [JsonPropertyName("hs")]
    public IDictionary<string, string> Headers { get; set; }

    [JsonPropertyName("ps")]
    public IDictionary<string, string> Params { get; set; }

    [JsonPropertyName("bd")]
    public string Body { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }
}

