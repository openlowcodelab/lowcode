using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

/// <summary>
/// 组件属性
/// </summary>
public class ComponentPropertyGroupSchema
{
    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    [JsonPropertyName("props")]
    public List<ComponentPropertySchema> Properties { get; set; } = [];
}