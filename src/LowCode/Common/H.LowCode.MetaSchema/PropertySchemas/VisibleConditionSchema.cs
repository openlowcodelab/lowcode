using System.Text.Json.Serialization;

namespace H.LowCode.MetaSchema;

/// <summary>
/// 组件显示条件
/// </summary>
/// <remarks>
/// 渲染时对条件求值，为真才渲染组件。
/// ValueExpr / ExpectExpr 均支持表达式，如 $(item.f_x)、$(form.key)、$(form[...])、$query(x)。
/// 用于组件显隐联动（如问题联动、按数据字段分支渲染）。
/// </remarks>
public class VisibleConditionSchema
{
    /// <summary>
    /// 值来源表达式
    /// </summary>
    [JsonPropertyName("vexpr")]
    public string? ValueExpr { get; set; }

    /// <summary>
    /// 比较操作符
    /// </summary>
    [JsonPropertyName("op")]
    public VisibleConditionOpEnum Op { get; set; } = VisibleConditionOpEnum.Equals;

    /// <summary>
    /// 期望值表达式
    /// </summary>
    [JsonPropertyName("eexpr")]
    public string? ExpectExpr { get; set; }
}

/// <summary>
/// 显示条件比较操作符
/// </summary>
public enum VisibleConditionOpEnum
{
    Equals = 0,
    NotEquals = 1,
    Contains = 2,
    NotEmpty = 3,
    IsEmpty = 4,

    /// <summary>
    /// 值在期望值列表中（期望值为逗号分隔列表）
    /// </summary>
    In = 5
}
