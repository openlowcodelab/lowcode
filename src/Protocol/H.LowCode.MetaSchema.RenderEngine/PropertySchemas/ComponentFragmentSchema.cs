using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentFragmentSchema : ComponentFragmentSchemaBase
{
    [JsonPropertyName("childs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public new ComponentFragmentSchema[] Childrens { get; set; }
}
