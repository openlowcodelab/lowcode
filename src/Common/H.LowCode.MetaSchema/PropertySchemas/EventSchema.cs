using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class EventSchema
{
    [JsonPropertyName("en")]
    public string EventName { get; set; }

    [JsonPropertyName("eht")]
    public EventHandlerTypeEnum EventHandlerType { get; set; }

    [JsonPropertyName("eat")]
    public int EventActionType { get; set; }

    [JsonPropertyName("eh")]
    public string EventHandler { get; set; }
}
