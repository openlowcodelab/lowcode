﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema;

public abstract class ComponentAttributeDefineSchemaBase
{
    [JsonPropertyName("attrn")]
    public string AttributeName { get; set; }

    /// <summary>
    /// 组件属性值 clr 类型
    /// </summary>
    [JsonPropertyName("attrt")]
    public string AttributeClrType { get; set; }

    [JsonPropertyName("attrv")]
    public object AttributeValue { get; set; }
}
