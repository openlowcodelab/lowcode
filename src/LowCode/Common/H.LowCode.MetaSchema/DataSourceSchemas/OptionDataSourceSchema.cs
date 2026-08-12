using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public record OptionDataSourceSchema
{
    [JsonIgnore]
    public string Id { get; set; } = ShortIdGenerator.Generate();

    [JsonPropertyName("l")]
    public string? Label { get; set; }

    [JsonPropertyName("v")]
    public string? Value { get; set; }

    [JsonPropertyName("s")]
    public bool IsSelected { get; set; }

    [JsonPropertyName("o")]
    public int Order { get; set; }

    [JsonPropertyName("g")]
    public string? Group { get; set; }

    [JsonPropertyName("d")]
    public string? Description { get; set; }
}
