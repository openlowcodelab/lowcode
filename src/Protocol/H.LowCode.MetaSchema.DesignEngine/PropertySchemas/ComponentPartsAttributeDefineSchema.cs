using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    public Dictionary<string, object> Options { get; set; }

    [JsonIgnore]
    public string StringValue
    {
        get
        {
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
            return (int)AttributeValue.ConvertToRealType(typeof(int));
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
            return (bool)AttributeValue.ConvertToRealType(typeof(bool));
        }
        set
        {
            AttributeValue = value;
        }
    }
}
