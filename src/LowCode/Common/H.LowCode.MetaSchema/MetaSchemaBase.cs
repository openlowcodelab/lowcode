using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public abstract class MetaSchemaBase : StateHasChangeSchema
{
    [JsonPropertyName("cid")]
    public string? CreatorId { get; set; }

    [JsonPropertyName("ct")]
    public DateTime? CreationTime { get; set; }

    [JsonPropertyName("mid")]
    public string? ModifierId { get; set; }

    [JsonPropertyName("mt")]
    public DateTime? ModificationTime { get; set; }
}
