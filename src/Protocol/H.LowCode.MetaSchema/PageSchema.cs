using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class PageSchema : MetaSchemaBase
{
    [JsonPropertyName("aid")]
    public string AppId { get; set; }

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
    public IList<ComponentSchema> Components { get; set; } = [];

    [JsonPropertyName("pageprop")]
    public PagePropertySchema PageProperty { get; set; } = new();

    [JsonPropertyName("ds")]
    public DataSourceSchema DataSource { get; set; } = new();

    public I18n I18n { get; set; }
}

public struct I18n
{
    public string Name { get; set; }

    public IDictionary<string, string> Properties { get; set; }
}
