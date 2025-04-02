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
            if (AttributeValue == null)
                return default;

            if (AttributeClrType != "System.String")
                return default;

            if (AttributeValue is JsonElement valueElement)
                return valueElement.GetString();
            else if (AttributeValue is string s)
                return s;

            return default;
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

            if (AttributeClrType != "System.Int32")
                return default;

            if (AttributeValue is JsonElement valueElement)
                return valueElement.GetInt32();
            else if (AttributeValue is Int32 i)
                return i;

            return default;
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

            if (AttributeClrType != "System.Boolean")
                return default;

            if (AttributeValue is JsonElement valueElement)
                return valueElement.GetBoolean();
            else if (AttributeValue is Boolean bol)
                return bol;

            return default;
        }
        set
        {
            AttributeValue = value;
        }
    }
}
