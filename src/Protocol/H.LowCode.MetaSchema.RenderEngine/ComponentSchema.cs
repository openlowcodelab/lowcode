using H.Extensions.System;
using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentSchema : ComponentSchemaBase
{
    /// <summary>
    /// 组件渲染 Fragment
    /// </summary>
    [JsonPropertyName("frag")]
    public new ComponentFragmentSchema Fragment { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("childs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public new ComponentSchema[] Childrens { get; set; }
}