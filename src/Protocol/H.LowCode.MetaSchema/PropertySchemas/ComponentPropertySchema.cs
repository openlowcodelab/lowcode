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

    [JsonPropertyName("t")]
    public string Title { get; set; }

    [JsonPropertyName("desc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Description { get; set; }

    [JsonPropertyName("max")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double? Maximum { get; set; }

    [JsonPropertyName("min")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double? Minimum { get; set; }

    [JsonPropertyName("maxlen")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long? MaximumLength { get; set; }

    [JsonPropertyName("minlen")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public long? MinimumLength { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Pattern { get; set; }

    [JsonPropertyName("compvaltype")]
    public ComponentValueType ComponentValueType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Format { get; set; }

    [JsonPropertyName("required")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsRequired { get; set; }

    [JsonPropertyName("ro")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ReadOnly { get; set; }

    [JsonPropertyName("default")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string DefaultValue { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string[] Enum { get; set; }

    [JsonPropertyName("enumnames")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string[] EnumNames { get; set; }

    /// <summary>
    /// 支持的通用属性
    /// </summary>
    [JsonPropertyName("supportprops")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IList<string> SupportProperties { get; set; } = [];

    /// <summary>
    /// 扩展的简单属性
    /// </summary>
    [JsonPropertyName("extprops")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IList<PropertyItemSchema> ExtensionProperties { get; set; } = [];

    /// <summary>
    /// 扩展的高级属性
    /// </summary>
    [JsonPropertyName("renderfragprop")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public RenderFragment RenderFragmentProperties { get; set; }

    [JsonPropertyName("extdata")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IDictionary<string, string> ExtensionData { get; set; } = new Dictionary<string, string>();

    #region method
    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public virtual bool IsShowProperty(string propertyName)
    {
        if (SupportProperties == null)
            return false;

        if (SupportProperties.Contains(propertyName))
            return true;
        return false;
    }
    #endregion
}

public enum ComponentValueType
{
    None = 0,
    String = 1,
    Number = 2,
    Integer = 3,
    Boolean = 4,
    Object = 5,
    Array = 6,
    Date = 7
}