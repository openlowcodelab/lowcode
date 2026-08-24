using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsFragmentSchema : ComponentFragmentSchemaBase
{
    /// <summary>
    /// 默认组件类型名。
    /// 原生 html 元素用 "html:{tag}" 形式（如 "html:button"、"html:input"）；
    /// .NET 组件为类型全名（如 "H.LowCode.Components.LcSelect, H.LowCode.Components"）
    /// </summary>
    [JsonPropertyName("dt")]
    public string? DefaultTypeName { get; set; }

    /// <summary>
    /// 组件类型名
    /// </summary>
    /// <remarks>
    /// 组件定义中使用 DefaultTypeName 即可，无需保存 TypeName
    /// 组件保存 json 文件时，强制设置 TypeName 为 null
    /// </remarks>
    [JsonPropertyName("t")]
    public override string? TypeName { get; set; }

    [JsonPropertyName("childs")]
    public ComponentPartsFragmentSchema[] ChildFragments { get; set; } = [];

    [JsonIgnore]
    public bool HasChildFragment
    {
        get
        {
            if (ChildFragments == null || ChildFragments.Length == 0)
                return false;
            return true;
        }
    }
}
