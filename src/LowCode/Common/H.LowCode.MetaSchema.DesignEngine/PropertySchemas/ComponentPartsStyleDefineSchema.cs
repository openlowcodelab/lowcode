using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema.DesignEngine;

public class ComponentPartsStyleDefineSchema
{
    /// <summary>
    /// 样式属性名称
    /// </summary>
    [JsonPropertyName("sn")]
    public string StyleName { get; set; }

    /// <summary>
    /// 样式显示名称
    /// </summary>
    [JsonPropertyName("disn")]
    public string DisplayName { get; set; }

    /// <summary>
    /// 样式描述
    /// </summary>
    [JsonPropertyName("desc")]
    public string Description { get; set; }

    /// <summary>
    /// 样式分组
    /// </summary>
    [JsonPropertyName("group")]
    public string Group { get; set; }

    /// <summary>
    /// CSS属性名
    /// </summary>
    [JsonPropertyName("cssprop")]
    public string CssProperty { get; set; }

    /// <summary>
    /// 样式类型
    /// </summary>
    [JsonPropertyName("st")]
    public string StyleType { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    [JsonPropertyName("dftval")]
    public string DefaultValue { get; set; }

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

    /// <summary>
    /// 选项配置
    /// </summary>
    [JsonPropertyName("options")]
    public string Options { get; set; }

    /// <summary>
    /// 控件类型
    /// </summary>
    [JsonPropertyName("controltype")]
    public string ControlType { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [JsonPropertyName("unit")]
    public string Unit { get; set; }
}