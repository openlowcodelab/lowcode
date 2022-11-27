using Newtonsoft.Json.Schema;
using System.Text.Json.Serialization;

namespace H.LowCode.Schema
{
    public class PageSchema
    {
        [JsonPropertyName("v")]
        public string Version { get; set; } = "1.0.0";

        [JsonPropertyName("c")]
        public JSchema PageContentSchema { get; set; }

        [JsonPropertyName("d")]
        public DataSource DataSource { get; set; }

        [JsonPropertyName("i18n")]
        public BaseSchema I18n { get; set; }
    }
}
