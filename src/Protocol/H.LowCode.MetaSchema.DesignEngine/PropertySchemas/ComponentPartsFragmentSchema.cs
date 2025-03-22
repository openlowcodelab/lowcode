using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsFragmentSchema : ComponentFragmentSchemaBase
{
    /// <summary>
    /// 默认组件类型名，如 "AntDesign.Input`1[System.String], AntDesign"
    /// </summary>
    [JsonPropertyName("dt")]
    public string DefaultTypeName { get; set; }

    /// <summary>
    /// 组件类型名
    /// </summary>
    /// <remarks>
    /// 组件定义中使用 DefaultTypeName 即可，无需保存 TypeName
    /// 组件保存 json 文件时，强制设置 TypeName 为 null
    /// </remarks>
    [JsonPropertyName("t")]
    public override string TypeName { get; set; }

    [JsonPropertyName("childs")]
    public ComponentPartsFragmentSchema[] Childrens { get; set; }
}
