using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

/// <summary>
/// 组件属性
/// </summary>
public class ComponentPropertySchema
{
    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    [JsonPropertyName("desc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Description { get; set; }

    [JsonPropertyName("dftval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public object DefaultValue { get; set; }

    [JsonPropertyName("strval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string StringValue { get; set; }

    [JsonPropertyName("boolval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? BoolValue { get; set; }

    [JsonPropertyName("intval")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? IntValue { get; set; }

    [JsonPropertyName("ops")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Dictionary<string, object> Options { get; }



    //[JsonPropertyName("max")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public double? Maximum { get; set; }

    //[JsonPropertyName("min")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public double? Minimum { get; set; }

    //[JsonPropertyName("maxlen")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public long? MaximumLength { get; set; }

    //[JsonPropertyName("minlen")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public long? MinimumLength { get; set; }

    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public string Pattern { get; set; }

    /// <summary>
    /// 组件值类型
    /// </summary>
    [JsonPropertyName("compvaltype")]
    public ComponentValueTypeEnum ComponentValueType { get; set; }

    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public string Format { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsRequired { get; set; }

    //[JsonPropertyName("ro")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public bool ReadOnly { get; set; }

    //[JsonPropertyName("default")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public string DefaultValue { get; set; }

    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public string[] Enum { get; set; }

    //[JsonPropertyName("enumnames")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public string[] EnumNames { get; set; }

    ///// <summary>
    ///// 选项类组件属性值
    ///// </summary>
    //[JsonPropertyName("ops")]
    //public string Options { get; set; }

    /// <summary>
    /// 表格类组件属性值
    /// </summary>
    [JsonPropertyName("tbprops")]
    public string TablePropertiess { get; set; }

    ///// <summary>
    ///// 扩展的属性
    ///// </summary>
    //[JsonPropertyName("extprops")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public IDictionary<string, PropertyItemSchema> ExtraProperties { get; set; } = new Dictionary<string, PropertyItemSchema>();

    //[JsonPropertyName("extdata")]
    //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    //public IDictionary<string, string> ExtensionData { get; set; } = new Dictionary<string, string>();

    #region method
    public virtual object GetDefaultValue()
    {
        switch (ComponentValueType)
        {
            case ComponentValueTypeEnum.Text:
            case ComponentValueTypeEnum.String:
            case ComponentValueTypeEnum.Textarea:
                return default(string);
            case ComponentValueTypeEnum.Boolean:
                return default(bool);
            case ComponentValueTypeEnum.Integer:
            case ComponentValueTypeEnum.Double:
            case ComponentValueTypeEnum.Decimal:
                return default(int);
            case ComponentValueTypeEnum.Date:
                return default(DateTime);
            default:
                return null;
        }
    }
    #endregion
}