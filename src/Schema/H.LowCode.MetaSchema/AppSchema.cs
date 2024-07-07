using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class AppSchema
    {
        [JsonPropertyName("id")]
        public string AppId { get; set; }

        [JsonPropertyName("ver")]
        public string Version { get; set; }

        [JsonPropertyName("pages")]
        public IList<PageSchema> Pages { get; set; }

        [JsonPropertyName("state")]
        public PublishStateEnum PublishState { get; set; }

        /// <summary>
        /// 支持平台  pc,mobile
        /// </summary>
        [JsonPropertyName("platform")]
        public string SupportPlatforms { get; set; }
    }

    public enum PublishStateEnum
    {
        Development,
        Approving,
        Published
    }
}
