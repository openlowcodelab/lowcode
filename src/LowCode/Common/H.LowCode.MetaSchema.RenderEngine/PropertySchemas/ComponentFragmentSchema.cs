using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentFragmentSchema : ComponentFragmentSchemaBase
{
    [JsonPropertyName("childs")]
    public ComponentFragmentSchema[] ChildFragments { get; set; }

    public bool HasChildren
    {
        get
        {
            if (ChildFragments == null || ChildFragments.Length == 0)
                return false;
            return true;
        }
    }
}
