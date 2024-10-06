using H.LowCode.MetaSchema;
using System.Text.Json.Serialization;

namespace H.LowCode.PartsMetaSchema;

public class PagePartsSchema : BasePartsMetaSchema
{
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("pt")]
    public PageTypeEnum PageType { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [JsonPropertyName("pubstatus")]
    public bool PublishStatus { get; set; }

    [JsonPropertyName("comps")]
    public IList<ComponentPartsSchema> Components { get; set; } = [];

    //[JsonPropertyName("pageprop")]
    //public PagePropertySchema PageProperty { get; set; } = new();
}
