
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class ComponentFragmentSchema
{
    [JsonPropertyName("idx")]
    public int Index { get; set; }

    [JsonPropertyName("frageenum")]
    public FragmentEnum FragmentEnum { get; set; }

    [JsonPropertyName("fragname")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string ComponentFragmentName { get; set; }

    [JsonPropertyName("n")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Name { get; set; }

    [JsonPropertyName("valtype")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public ComponentValueTypeEnum ValueType { get; set; }

    [JsonPropertyName("intval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IntValue { get; set; }

    [JsonPropertyName("strval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string StringValue { get; set; }

    [JsonPropertyName("strvals")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<string> StringValues { get; set; }
}

public enum FragmentEnum
{
    Component,
    Attribute,
    Parameter
}
