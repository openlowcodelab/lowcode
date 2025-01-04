using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class ComponentFragmentSchema
{
    /// <summary>
    /// 组件类型完整限定名
    /// </summary>
    [JsonPropertyName("t")]
    public virtual string FullTypeName { get; set; }

    [JsonPropertyName("valt")]
    public string ValueType { get; set; }

    [JsonPropertyName("ts")]
    public IDictionary<string, string> FullTypeOptions { get; set; }

    [JsonPropertyName("params")]
    public IList<ComponentParameterFragmentSchema> Parameters { get; set; } = [];

    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Content { get; set; }

    public object GetDefaultValue()
    {
        if (string.IsNullOrEmpty(ValueType))
            return null;

        Type type = Type.GetType(ValueType);
        // 值类型
        if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
        {
            return Activator.CreateInstance(type);
        }

        // 引用类型
        return null;
    }
}

public record ComponentParameterFragmentSchema
{
    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("valt")]
    public ComponentValueTypeEnum ValueType { get; set; }

    [JsonPropertyName("intv")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IntValue { get; set; }

    [JsonPropertyName("strv")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string StringValue { get; set; }

    [JsonPropertyName("strvs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<string> StringValues { get; set; }
}
