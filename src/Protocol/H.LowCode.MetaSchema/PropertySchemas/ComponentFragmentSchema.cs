using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class ComponentFragmentSchema
{
    /// <summary>
    /// 默认组件类型完整限定名，如 AntDesign.Input`1[System.String], AntDesign
    /// </summary>
    [JsonPropertyName("dtn")]
    public string DefaultFullTypeName { get; set; }

    private string _fullTypeName;
    /// <summary>
    /// 组件类型完整限定名
    /// </summary>
    [JsonPropertyName("tn")]
    public string FullTypeName
    {
        get
        {
            if (string.IsNullOrEmpty(_fullTypeName))
                return DefaultFullTypeName;
            return _fullTypeName;
        }
        set
        {
            _fullTypeName = value;
        }
    }

    [JsonPropertyName("tm")]
    public string TModel { get; set; }

    [JsonPropertyName("tops")]
    public IDictionary<string, string> FullTypeOptions { get; set; }

    [JsonPropertyName("params")]
    public IList<ComponentParameterFragmentSchema> Parameters { get; set; } = [];

    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Content { get; set; }

    public object GetDefaultValue()
    {
        Type type = Type.GetType(TModel);
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
