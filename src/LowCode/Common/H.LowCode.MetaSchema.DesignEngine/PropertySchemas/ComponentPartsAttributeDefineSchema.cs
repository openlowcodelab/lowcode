using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsAttributeDefineSchema : ComponentAttributeDefineSchemaBase
{
    [JsonPropertyName("disn")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 设置项类型 (用于判断设置项控件渲染)
    /// </summary>
    [JsonPropertyName("pt")]
    public ComponentAttributeItemTypeEnum AttributeItemType { get; set; }

    [JsonPropertyName("required")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("desc")]
    public string? Description { get; set; }

    [JsonPropertyName("dftval")]
    public object? DefaultValue { get; set; }

    [JsonPropertyName("ops")]
    public Dictionary<string, object>? Options { get; set; }

    /// <summary>
    /// 是否启用校验
    /// </summary>
    [JsonPropertyName("enableval")]
    public bool IsValidationEnabled { get; set; }

    /// <summary>
    /// 校验规则
    /// </summary>
    [JsonPropertyName("valrules")]
    public IList<ValidationRuleSchema>? ValidationRules { get; set; }

    [JsonIgnore]
    public string? StringValue
    {
        get
        {
            if (AttributeValue == null)
                return null;

            return AttributeValue.ConvertToRealType(typeof(string))?.ToString();
        }
        set
        {
            if (value == null)
                return;

            AttributeValue = value;
        }
    }

    [JsonIgnore]
    public int IntValue
    {
        get
        {
            if (AttributeValue == null)
                return 0;

            var val = AttributeValue.ConvertToRealType(typeof(int));
            if (val == null)
                return 0;

            return (int)val;
        }
        set
        {
            AttributeValue = value;
        }
    }

    [JsonIgnore]
    public bool BoolValue
    {
        get
        {
            if (AttributeValue == null)
                return false;

            var val = AttributeValue.ConvertToRealType(typeof(bool));
            if (val == null)
                return false;

            return (bool)val;
        }
        set
        {
            AttributeValue = value;
        }
    }
}
