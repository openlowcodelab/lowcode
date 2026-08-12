using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.RenderEngine;

public class ComponentDataSourceSchema : ComponentDataSourceSchemaBase
{
    /// <summary>
    /// DataSource 渲染的 Fragment
    /// </summary>
    [JsonPropertyName("dsfrag")]
    public ComponentFragmentSchema DataSourceFragment { get; set; }

    /// <summary>
    /// 列表项模板（支持完整的组件配置，包括条件渲染）
    /// </summary>
    [JsonPropertyName("itemtpl")]
    public ComponentSchema ItemTemplate { get; set; }
}
