using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class AppSchema : MetaSchemaBase
{
    public string Id { get; set; }

    [JsonPropertyName("n")]
    public string Name { get; set; }

    public string Icon { get; set; }

    [JsonPropertyName("pic")]
    public string Picture { get; set; }

    [JsonPropertyName("desc")]
    public string Description {  get; set; }

    [JsonPropertyName("v")]
    public string Version { get; set; }

    [JsonPropertyName("pubstatus")]
    public PublishStatusEnum PublishStatus { get; set; }

    [JsonPropertyName("platform")]
    public SupportPlatformEnum[] SupportPlatforms { get; set; } = [0];
}

public enum PublishStatusEnum
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
