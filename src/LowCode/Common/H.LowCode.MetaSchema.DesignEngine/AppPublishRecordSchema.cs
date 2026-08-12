using H.Util.Ids;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

/// <summary>
/// 应用发布记录
/// </summary>
public class AppPublishRecordSchema
{
    [JsonPropertyName("aid")]
    public string AppId { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; } = ShortIdGenerator.Generate();

    /// <summary>
    /// 发布版本号
    /// </summary>
    [JsonPropertyName("v")]
    public string Version { get; set; }

    /// <summary>
    /// 发布状态：Published / Rollback
    /// </summary>
    [JsonPropertyName("status")]
    public AppPublishStatusEnum Status { get; set; } = AppPublishStatusEnum.Published;

    /// <summary>
    /// 发布说明
    /// </summary>
    [JsonPropertyName("desc")]
    public string Description { get; set; }

    /// <summary>
    /// 操作人
    /// </summary>
    [JsonPropertyName("op")]
    public string Operator { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    [JsonPropertyName("pt")]
    public DateTime PublishTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 快照页面数
    /// </summary>
    [JsonPropertyName("pc")]
    public int PageCount { get; set; }
}

public enum AppPublishStatusEnum
{
    Published = 0,
    Rollback = 1
}
