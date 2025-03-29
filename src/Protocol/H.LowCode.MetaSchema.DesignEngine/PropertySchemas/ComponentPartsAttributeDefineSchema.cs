using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsAttributeDefineSchema : ComponentAttributeDefineSchemaBase
{
    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 设置项类型 (用于判断设置项控件渲染)
    /// </summary>
    [JsonPropertyName("pt")]
    public ComponentAttributeItemTypeEnum AttributeItemType { get; set; }

    [JsonPropertyName("required")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }

    [JsonPropertyName("dftval")]
    public object DefaultValue { get; set; }

    [JsonPropertyName("ops")]
    public Dictionary<string, object> Options { get; }

    [JsonIgnore]
    public string StringValue
    {
        get
        {
            return AttributeValue?.ToString();
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
                return default;

            return (int)AttributeValue;
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
                return default;

            return (bool)AttributeValue;
        }
        set
        {
            AttributeValue = value;
        }
    }
}
