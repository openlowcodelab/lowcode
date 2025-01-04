using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.PartsMetaSchema;

public class ComponentPartsFragmentSchema : ComponentFragmentSchema
{
    /// <summary>
    /// 默认组件类型完整限定名，如 "AntDesign.Input`1[System.String], AntDesign"
    /// </summary>
    [JsonPropertyName("dt")]
    public string DefaultFullTypeName { get; set; }

    /// <summary>
    /// 组件类型完整限定名
    /// </summary>
    /// <remarks>
    /// 组件定义中使用 DefaultFullTypeName 即可，无需保存 FullTypeName
    /// 组件保存 json 文件时，强制设置 FullTypeName 为 null
    /// </remarks>
    [JsonPropertyName("t")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public override string FullTypeName { get; set; }
}
