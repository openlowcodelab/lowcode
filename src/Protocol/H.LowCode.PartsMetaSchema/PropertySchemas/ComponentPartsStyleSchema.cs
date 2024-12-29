using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace H.LowCode.PartsMetaSchema;

public class ComponentPartsStyleSchema
{
    /// <summary>
    /// 组件宽度
    /// </summary>
    /// <remarks>有效值为4-24，如 12/24*100 = 50%。有值以当前值为准,无值以页面布局为准</remarks>
    [JsonPropertyName("itemw")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ItemWidth { get; set; }

    /// <summary>
    /// 组件高度 (单位:px)
    /// </summary>
    /// <remarks>默认85px</remarks>
    [JsonPropertyName("itemh")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double ItemHeight { get; set; } = 85;

    /// <summary>
    /// 标签宽度（默认 180 px）
    /// </summary>
    [JsonPropertyName("labelw")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double LabelWidth { get; set; } = 180;

    /// <summary>
    /// 默认样式
    /// </summary>
    [JsonPropertyName("defstyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string DefaultStyle { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Display { get; set; } = "inline";

    [JsonPropertyName("pos")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string Position { get; set; } = "static";

    /// <summary>
    /// 自定义样式
    /// </summary>
    [JsonPropertyName("cusstyle")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string CustomStyle { get; set; }
}
