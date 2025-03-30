using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public abstract class ComponentAttributeDefineSchemaBase
{
    /// <summary>
    /// 组件属性名称
    /// </summary>
    /// <remarks>必须为组件中存在的属性名称, 不可随意命名</remarks>
    [JsonPropertyName("attrn")]
    public string AttributeName { get; set; }

    /// <summary>
    /// 组件属性值 clr 类型
    /// </summary>
    [JsonPropertyName("attrt")]
    public string AttributeClrType { get; set; }

    /// <summary>
    /// 组件属性对应的值
    /// </summary>
    [JsonPropertyName("attrv")]
    public object AttributeValue { get; set; }
}
