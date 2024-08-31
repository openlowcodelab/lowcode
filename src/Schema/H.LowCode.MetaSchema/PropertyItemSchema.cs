using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class PropertyItemSchema
{
    [JsonPropertyName("label")]
    public string Label { get; set; }

    [JsonPropertyName("strval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string StringValue { get; set; }

    [JsonPropertyName("boolval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool BoolValue { get; set; }

    [JsonPropertyName("intval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IntValue { get; set; }

    [JsonPropertyName("desc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Description { get; set; }

    /// <summary>
    /// 设置项类型
    /// </summary>
    [JsonPropertyName("itemtype")]
    public PropertyItemTypeEnum SettingItemType { get; set; }
}

public enum PropertyItemTypeEnum
{
    Text,
    Text_Int,
    Radio,
    Checkbox,
    Select,
    Switch,
    Date,
    Textarea,
    Options
}
