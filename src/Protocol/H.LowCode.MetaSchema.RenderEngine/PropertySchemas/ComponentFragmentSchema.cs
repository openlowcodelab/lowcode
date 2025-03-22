using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentFragmentSchema : ComponentFragmentSchemaBase
{
    [JsonPropertyName("childs")]
    public ComponentFragmentSchema[] Childrens { get; set; }
}
