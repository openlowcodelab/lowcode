using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema
{
    public class AppSchema
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("n")]
        public string Name { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("pic")]
        public string Picture { get; set; }

        [JsonPropertyName("desc")]
        public string Description {  get; set; }

        [JsonPropertyName("v")]
        public string Version { get; set; }

        [JsonPropertyName("menus")]
        public IList<MenuSchema> Menus { get; set; }

        [JsonPropertyName("state")]
        public PublishStateEnum PublishState { get; set; }

        [JsonPropertyName("platform")]
        public SupportPlatformEnum[] SupportPlatforms { get; set; }
    }

    public enum PublishStateEnum
    {
        Development,
        Approving,
        Published
    }

    public enum SupportPlatformEnum
    {
        [Display(Name = "Web")]
        Web,
        [Display(Name = "App")]
        Mobile,
        [Display(Name = "小程序")]
        WXMiniApp
    }
}
