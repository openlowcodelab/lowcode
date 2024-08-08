using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class MenuSchema
    {
        [JsonPropertyName("aid")]
        public string AppId {  get; set; }

        public string Id { get; set; }

        [JsonPropertyName("t")]
        public string Title { get; set; }

        public string Icon {  get; set; }

        public string MenuUrl { get; set; }

        public int Order { get; set; }
    }
}
