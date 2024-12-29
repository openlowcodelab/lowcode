using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class PropertyItemSchema
{
    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("desc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Description { get; set; }

    public int Order { get; set; }

    [JsonPropertyName("strval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string StringValue { get; set; }

    [JsonPropertyName("boolval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? BoolValue { get; set; }

    [JsonPropertyName("intval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? IntValue { get; set; }

    public IDictionary<string, string> Options { get; set; }

    /// <summary>
    /// 设置项类型
    /// </summary>
    [JsonPropertyName("itemtype")]
    public PropertyItemTypeEnum SettingItemType { get; set; }
}
