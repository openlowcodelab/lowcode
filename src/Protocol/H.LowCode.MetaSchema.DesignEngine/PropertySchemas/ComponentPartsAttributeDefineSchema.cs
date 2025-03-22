using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsAttributeDefineSchema
{
    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 设置项类型 (用于判断设置项控件渲染)
    /// </summary>
    [JsonPropertyName("pt")]
    public ComponentAttributeItemTypeEnum AttributeItemType { get; set; }

    /// <summary>
    /// 组件值类型
    /// </summary>
    [JsonPropertyName("compvaltype")]
    public ComponentValueTypeEnum ComponentValueType { get; set; }

    [JsonPropertyName("required")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }

    [JsonPropertyName("dftval")]
    public object DefaultValue { get; set; }

    [JsonPropertyName("strval")]
    public string StringValue { get; set; }

    [JsonPropertyName("boolval")]
    public bool? BoolValue { get; set; }

    [JsonPropertyName("intval")]
    public int? IntValue { get; set; }

    [JsonPropertyName("ops")]
    public Dictionary<string, object> Options { get; }
}
