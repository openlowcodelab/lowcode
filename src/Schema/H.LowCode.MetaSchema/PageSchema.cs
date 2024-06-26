using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class PageSchema
    {
        [JsonPropertyName("appid")]
        public string AppId { get; set; }

        [JsonPropertyName("id")]
        public string PageId { get; set; }

        [JsonPropertyName("c")]
        public IList<ComponentSchema> Components { get; set; }

        [JsonPropertyName("p")]
        public PagePropertySchema PageProperty { get; set; }

        [JsonPropertyName("d")]
        public DataSourceSchema DataSource { get; set; }

        [JsonPropertyName("i")]
        public I18n I18n { get; set; }
    }

    public enum PageTypeEnum
    {
        None,
        Form,
        Table
    }

    public struct I18n
    {
        public string Name { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}
