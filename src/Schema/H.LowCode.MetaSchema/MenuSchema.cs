using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class MenuSchema
    {
        [JsonPropertyName("id")]
        public string MenuId { get; set; }

        [JsonPropertyName("n")]
        public string MenuName { get; set; }

        [JsonPropertyName("t")]
        public string MenuTitle { get; set; }
    }
}
