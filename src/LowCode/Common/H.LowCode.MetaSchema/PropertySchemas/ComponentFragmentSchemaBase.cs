using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

public abstract class ComponentFragmentSchemaBase
{
    /// <summary>
    /// 组件类型名
    /// </summary>
    [JsonPropertyName("t")]
    public virtual string? TypeName { get; set; }

    [JsonPropertyName("valt")]
    public string? ValueType { get; set; }

    [JsonPropertyName("attrs")]
    public ComponentAttributeFragmentSchema[] Attributes { get; set; } = [];

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// 事件配置（用于按钮等组件的事件绑定）
    /// </summary>
    [JsonPropertyName("evs")]
    public IList<EventSchema>? Events { get; set; }

    /// <summary>
    /// 静态资源依赖清单（js / css），渲染该片段时按序加载。
    /// 用于编辑器等需要外部 js/css 的纯物料组件。
    /// </summary>
    [JsonPropertyName("res")]
    public ResourceItemSchema[]? Resources { get; set; }

    /// <summary>
    /// 资源加载完成后的初始化函数名（js 模块导出），以挂载点 id + 组件选项调用。
    /// </summary>
    [JsonPropertyName("init")]
    public string? InitFunction { get; set; }

    public object? GetDefaultValue()
    {
        if (string.IsNullOrEmpty(ValueType))
            return null;

        Type type = Type.GetType(ValueType);
        return type.GetDefaultValue();
    }
}

/// <summary>
/// 静态资源依赖项
/// </summary>
public record ResourceItemSchema
{
    /// <summary>
    /// 资源类型：css / js
    /// </summary>
    [JsonPropertyName("t")]
    public string? Type { get; set; }

    /// <summary>
    /// 资源地址（外部 URL、_content 路径或已上传文件的访问地址）
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public record ComponentAttributeFragmentSchema
{
    [JsonPropertyName("attrn")]
    public string? AttributeName { get; set; }

    /// <summary>
    /// 组件属性值 clr 类型
    /// </summary>
    [JsonPropertyName("attrt")]
    public string? AttributeClrType { get; set; }

    [JsonPropertyName("attrv")]
    public object? AttributeValue { get; set; }
}
