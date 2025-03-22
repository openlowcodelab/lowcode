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
    public ComponentFragmentSchema Fragment { get; set; }

    [JsonPropertyName("ds")]
    public ComponentDataSourceSchema DataSource { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("childs")]
    public ComponentSchema[] Childrens { get; set; }
}