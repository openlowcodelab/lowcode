using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public abstract class PageSchemaBase : MetaSchemaBase
{
    [JsonPropertyName("aid")]
    public string AppId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = ShortIdGenerator.Generate();

    [JsonPropertyName("n")]
    public string Name { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("pt")]
    public PageTypeEnum PageType { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }

    [JsonPropertyName("pageprop")]
    public PagePropertySchema PageProperty { get; set; } = new();

    [JsonPropertyName("ds")]
    public PageDataSourceSchema DataSource { get; set; } = new();
}