using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public class ComponentStyleSchema
{
    /// <summary>
    /// 组件宽度
    /// </summary>
    /// <remarks>有效值为4-24，如 12/24*100 = 50%。有值以当前值为准,无值以页面布局为准</remarks>
    [JsonPropertyName("itemw")]
    public double ItemWidth { get; set; } = 4;

    /// <summary>
    /// 组件高度 (单位:px)
    /// </summary>
    /// <remarks>默认85px</remarks>
    [JsonPropertyName("itemh")]
    public double ItemHeight { get; set; } = 85;

    /// <summary>
    /// 标签宽度（默认 180 px）
    /// </summary>
    [JsonPropertyName("labelw")]
    public double LabelWidth { get; set; } = 180;

    /// <summary>
    /// 默认样式
    /// </summary>
    [JsonPropertyName("dfstl")]
    public string? DefaultStyle { get; set; }

    /// <summary>
    /// 自定义样式
    /// </summary>
    [JsonPropertyName("ctstl")]
    public string? CustomStyle { get; set; }
}
