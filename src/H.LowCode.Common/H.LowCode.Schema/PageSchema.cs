using System.Text.Json.Serialization;

namespace H.LowCode.Schema
{
    public class PageSchema
    {
        [JsonPropertyName("v")]
        public string Version { get; set; } = "1.0.0";

        [JsonPropertyName("c")]
        public IList<ComponentPropertySchema> ComponentSchemas { get; set; }

        [JsonPropertyName("p")]
        public PagePropertySchema PagePropertySchema { get; set; }

        [JsonPropertyName("d")]
        public DataSource DataSource { get; set; }

        [JsonPropertyName("i")]
        public I18n I18n { get; set; }
    }

    public struct I18n
    {
        public string Name { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}
