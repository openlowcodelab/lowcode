using H.LowCode.MetaSchema;
using H.Util.Ids;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace H.LowCode.PartsMetaSchema;

public class ComponentLibrarySchema : PartsMetaSchemaBase
{
    /// <summary>
    /// 组件库Id
    /// </summary>
    [JsonPropertyName("libid")]
    public string LibraryId { get; set; }

    /// <summary>
    /// 组件库名称
    /// </summary>
    [JsonPropertyName("libname")]
    public string LibraryName { get; set; }

    /// <summary>
    /// 发布状态
    /// </summary>
    [JsonPropertyName("pub")]
    public int PublishStatus { get; set; }

    [JsonPropertyName("desc")]
    public string Description { get; set; }

    /// <summary>
    /// 支持平台
    /// </summary>
    [JsonPropertyName("platform")]
    public SupportPlatformEnum[] SupportPlatforms { get; set; } = [0];
}