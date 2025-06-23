using System;
using System.Text.Json.Serialization;

namespace H.LowCode.DesignEngine.Model;

public class ComponentListModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("lb")]
    public string Label { get; set; }
}
