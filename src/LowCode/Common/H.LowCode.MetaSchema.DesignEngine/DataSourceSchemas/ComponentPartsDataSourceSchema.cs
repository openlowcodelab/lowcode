using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsDataSourceSchema : ComponentDataSourceSchemaBase
{
    /// <summary>
    /// DataSource 渲染的 Fragment
    /// </summary>
    [JsonPropertyName("dsfrag")]
    public ComponentPartsFragmentSchema? DataSourceFragment { get; set; }

    /// <summary>
    /// 列表项模板（支持完整的组件配置，包括条件渲染）
    /// </summary>
    [JsonPropertyName("itemtpl")]
    public ComponentPartsSchema? ItemTemplate { get; set; }
}
