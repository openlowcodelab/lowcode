using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class MenuSchema
    {
        [JsonPropertyName("aid")]
        public string AppId {  get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("t")]
        public string Title { get; set; }

        [JsonPropertyName("icon")]
        public string Icon {  get; set; }
    }
}
