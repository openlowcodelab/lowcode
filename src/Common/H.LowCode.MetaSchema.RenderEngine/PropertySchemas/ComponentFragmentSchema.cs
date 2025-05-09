using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentFragmentSchema : ComponentFragmentSchemaBase
{
    [JsonPropertyName("childs")]
    public ComponentFragmentSchema[] Childrens { get; set; }

    public bool HasChildren
    {
        get
        {
            if (Childrens == null || Childrens.Length == 0)
                return false;
            return true;
        }
    }
}
