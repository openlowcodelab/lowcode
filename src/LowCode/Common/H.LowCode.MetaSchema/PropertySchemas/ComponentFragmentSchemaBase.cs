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

    public object? GetDefaultValue()
    {
        if (string.IsNullOrEmpty(ValueType))
            return null;

        Type type = Type.GetType(ValueType);
        return type.GetDefaultValue();
    }
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
