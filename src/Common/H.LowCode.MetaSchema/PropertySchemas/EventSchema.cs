using System;
using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class EventSchema
{
    [JsonPropertyName("en")]
    public string EventName { get; set; }

    [JsonPropertyName("eht")]
    public EventTargetTypeEnum EventHandlerType { get; set; }

    #region 标准事件
    /// <summary>
    /// 事件目标id (如页面id、组件id等)
    /// </summary>
    [JsonPropertyName("etid")]
    public string EventTargetId { get; set; }

    /// <summary>
    /// 事件目标动作
    /// </summary>
    [JsonPropertyName("eta")]
    public string EventTargetAction { get; set; }
    #endregion

    #region 自定义事件
    /// <summary>
    /// 自定义脚本语言
    /// </summary>
    [JsonPropertyName("ecl")]
    public EventCustomLanguageEnum EventCustomLanguage { get; set; }

    /// <summary>
    /// 自定义脚本内容
    /// </summary>
    [JsonPropertyName("ecs")]
    public string EventCustomScript { get; set; }
    #endregion

    /// <summary>
    /// 事件参数
    /// </summary>
    public IDictionary<string, string> EventArgs { get; set; }
}

public class EventConsumeSchema
{
    public string EventName { get; set; }

    public string EventDisplayName { get; set; }
}