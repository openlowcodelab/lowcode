using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.PartsMetaSchema;

public class ComponentPartsPropertySchema : ComponentPropertySchema
{
    /// <summary>
    /// 设置项类型 (用于判断设置项控件渲染)
    /// </summary>
    [JsonPropertyName("pt")]
    public PropertyItemTypeEnum PropertyItemType { get; set; }
}
