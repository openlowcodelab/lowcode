using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

/// <summary>
/// 页面库物料（可复用的页面模板）
/// </summary>
public class PageTemplateSchema : PartsMetaSchemaBase
{
    /// <summary>
    /// 页面模板Id
    /// </summary>
    [JsonPropertyName("tid")]
    public required string TemplateId { get; set; }

    [JsonPropertyName("n")]
    public string? Name { get; set; }

    /// <summary>
    /// 模板分类
    /// </summary>
    [JsonPropertyName("cat")]
    public PageTemplateCategoryEnum Category { get; set; }

    [JsonPropertyName("desc")]
    public string? Description { get; set; }

    /// <summary>
    /// 页面类型
    /// </summary>
    [JsonPropertyName("pt")]
    public PageTypeEnum PageType { get; set; }

    /// <summary>
    /// 组件树
    /// </summary>
    [JsonPropertyName("comps")]
    public IList<ComponentPartsSchema> Components { get; set; } = [];

    [JsonPropertyName("pageprop")]
    public PagePropertySchema PageProperty { get; set; } = new();

    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }
}
