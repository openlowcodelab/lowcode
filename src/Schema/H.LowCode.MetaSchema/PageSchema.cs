using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class PageSchema
    {
        [JsonPropertyName("aid")]
        public string AppId { get; set; }

        public string Id { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("pt")]
        public PageTypeEnum PageType { get; set; }

        [JsonPropertyName("comps")]
        public IList<ComponentSchema> Components { get; set; }

        [JsonPropertyName("pageprop")]
        public PagePropertySchema PageProperty { get; set; }

        [JsonPropertyName("ds")]
        public DataSourceSchema DataSource { get; set; }

        public I18n I18n { get; set; }
    }

    public enum PageTypeEnum
    {
        Normal,
        Form,
        Table,
        Report
    }

    public struct I18n
    {
        public string Name { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}
