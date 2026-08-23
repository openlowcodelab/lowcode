using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentFragmentSchema : ComponentFragmentSchemaBase
{
    /// <summary>
    /// 默认组件类型名（设计端落盘字段）。
    /// 普通组件为 .NET 类型全名（如 "H.LowCode.Components.Defaults.HcSelect, H.LowCode.Components.Defaults"），
    /// 原生 html 元素为 "html:{tag}" 形式（如 "html:button"）。
    /// TypeName(t) 为空时以其为回退。
    /// </summary>
    [JsonPropertyName("dt")]
    public string? DefaultTypeName { get; set; }

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
