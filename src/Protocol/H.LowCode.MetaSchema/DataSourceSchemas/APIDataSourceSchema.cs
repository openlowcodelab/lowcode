using H.Util.Ids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public class APIDataSourceSchema
{
    [JsonPropertyName("d")]
    public string Domain { get; set; }

    [JsonPropertyName("p")]
    public string Path { get; set; }

    [JsonPropertyName("m")]
    public string Method { get; set; }

    [JsonPropertyName("qs")]
    public IList<APIParamSchema> Queries { get; set; } = [];

    [JsonPropertyName("bd")]
    public APIBodySchema Body { get; set; } = new();

    [JsonPropertyName("hs")]
    public IList<APIParamSchema> Headers { get; set; } = [];
}

public record APIParamSchema
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = ShortIdGenerator.Generate();

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("t")]
    public string Type { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set;}
}

public class APIBodySchema
{
    public APIBodyTypeEnum DataType { get; set; }

    public string Value { get; set; }

    public IList<APIParamSchema> MultipartParams { get; set; } = [];
}

public enum APIBodyTypeEnum
{
    None = 0,
    Json = 1,
    Text = 2,
    Multipart = 3,
    Raw = 4,
    Baniry = 5
}
