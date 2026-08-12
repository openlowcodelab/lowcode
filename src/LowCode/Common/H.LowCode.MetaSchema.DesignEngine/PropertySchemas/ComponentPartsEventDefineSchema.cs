using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsEventDefineSchema
{
    /// <summary>
    /// 事件名称
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    [JsonPropertyName("en")]
    public string EventName { get; set; }

    /// <summary>
    /// 事件显示名称
    /// </summary>
    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 事件描述
    /// </summary>
    [JsonPropertyName("desc")]
    public string Description { get; set; }

    /// <summary>
    /// 事件分组
    /// </summary>
    [JsonPropertyName("group")]
    public string Group { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    [JsonPropertyName("et")]
    public string EventType { get; set; }

    /// <summary>
    /// 事件参数类型
    /// </summary>
    [JsonPropertyName("pt")]
    public string ParameterType { get; set; }

    /// <summary>
    /// 是否必需
    /// </summary>
    [JsonPropertyName("required")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }
}