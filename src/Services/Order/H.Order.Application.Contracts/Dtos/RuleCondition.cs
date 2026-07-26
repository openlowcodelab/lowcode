using System.Text.Json;

namespace H.Order.Application.Contracts;

/// <summary>
/// 路由规则条件（最小规则引擎结构）。
/// ConditionsJson 存储的是 <see cref="List{RuleCondition}"/> 的 JSON。
/// 多个条件之间是与（AND）关系，全部命中才视为该规则命中。
/// </summary>
public class RuleCondition
{
    /// <summary>
    /// 匹配字段：Industry / ProductCategory / TotalAmount / OrderStatus
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// 操作符：eq, ne, in, gt, lt, gte, lte, between, contains
    /// </summary>
    public string Op { get; set; } = "eq";

    /// <summary>
    /// 目标值。
    /// eq/ne/contains 取单值；in 取逗号分隔多值；gt/lt/gte/lte 取数值；between 取 "min,max"
    /// </summary>
    public string? Value { get; set; }
}

/// <summary>
/// 规则条件序列化/反序列化助手
/// </summary>
public static class RuleConditionHelper
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    /// <summary>将条件列表序列化为 JSON</summary>
    public static string ToJson(List<RuleCondition>? conditions) =>
        conditions is null || conditions.Count == 0
            ? "[]"
            : JsonSerializer.Serialize(conditions, Options);

    /// <summary>从 JSON 反序列化为条件列表</summary>
    public static List<RuleCondition> FromJson(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? new List<RuleCondition>()
            : JsonSerializer.Deserialize<List<RuleCondition>>(json, Options) ?? new List<RuleCondition>();
}