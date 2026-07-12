using H.Order.Application.Contracts;
using H.Order.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories;

namespace H.Order.Application.Services;

/// <summary>
/// 路由规则引擎接口：根据订单属性匹配应该下发给哪个供应商
/// </summary>
public interface IRouteEngine
{
    /// <summary>匹配命中的供应商编码（按优先级递增、AND 组合条件），命中返回值否则返回 null</summary>
    Task<string?> MatchByOrderAsync(OrderEntity order);
}

/// <summary>
/// 路由规则引擎实现（规则引擎思想 + 策略模式：与订单属性比对、按优先级排序）。
/// 加载所有启用的规则按优先级升序逐条评估，命中第一条即返回；
/// 兜底规则（Fallback=true 且 ConditionsJson 为空）作为终极匹配。
/// </summary>
public class RouteEngine : IRouteEngine
{
    private readonly IRepository<RouteRuleEntity, Guid> _ruleRepo;
    private readonly IRepository<SupplierEntity, Guid> _supplierRepo;

    public RouteEngine(
        IRepository<RouteRuleEntity, Guid> ruleRepo,
        IRepository<SupplierEntity, Guid> supplierRepo)
    {
        _ruleRepo = ruleRepo;
        _supplierRepo = supplierRepo;
    }

    public async Task<string?> MatchByOrderAsync(OrderEntity order)
    {
        var query = await _ruleRepo.GetQueryableAsync();
        var rules = await query.Where(x => x.IsEnabled).OrderBy(x => x.Priority).ToListAsync();

        // 构造启用的供应商编码集合，避免向已禁用的供应商下发
        var supplierQuery = await _supplierRepo.GetQueryableAsync();
        var enabledSupplierCodes = await supplierQuery
            .Where(x => x.IsEnabled)
            .Select(x => x.Code)
            .ToListAsync();

        string? fallbackCode = null;

        foreach (var rule in rules)
        {
            if (!enabledSupplierCodes.Contains(rule.SupplierCode))
            {
                continue;
            }

            if (rule.Fallback)
            {
                fallbackCode ??= rule.SupplierCode;
                continue;
            }

            var conditions = RuleConditionHelper.FromJson(rule.ConditionsJson);
            if (conditions.Count == 0)
            {
                // 无条件视为始终命中（兜底规则的一种）
                fallbackCode ??= rule.SupplierCode;
                continue;
            }

            if (conditions.All(c => Evaluate(c, order)))
            {
                return rule.SupplierCode;
            }
        }

        return fallbackCode;
    }

    /// <summary>
    /// 评估单个条件是否命中
    /// </summary>
    private static bool Evaluate(RuleCondition condition, OrderEntity order)
    {
        var value = condition.Value ?? string.Empty;
        var op = (condition.Op ?? "eq").ToLowerInvariant();

        switch (condition.Field)
        {
            case "Industry":
                return CompareString(op, order.Industry, value);
            case "ProductCategory":
                return CompareString(op, order.ProductCategory, value);
            case "TotalAmount":
                if (decimal.TryParse(value, out var dv))
                {
                    return CompareNumber(op, order.TotalAmount, dv, value);
                }
                return false;
            case "OrderStatus":
                if (int.TryParse(value, out var sv))
                {
                    return CompareNumber(op, order.OrderStatus, sv, value);
                }
                return false;
            default:
                // 未知字段保守视为不命中
                return false;
        }
    }

    private static bool CompareString(string op, string? actual, string value)
    {
        var actualVal = actual ?? string.Empty;
        return op switch
        {
            "eq" => string.Equals(actualVal, value, StringComparison.OrdinalIgnoreCase),
            "ne" => !string.Equals(actualVal, value, StringComparison.OrdinalIgnoreCase),
            "in" => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                         .Any(v => string.Equals(v, actualVal, StringComparison.OrdinalIgnoreCase)),
            "contains" => actualVal.Contains(value, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static bool CompareNumber(string op, decimal actual, decimal value, string rawValue)
    {
        return op switch
        {
            "eq" => actual == value,
            "ne" => actual != value,
            "gt" => actual > value,
            "lt" => actual < value,
            "gte" => actual >= value,
            "lte" => actual <= value,
            "between" => ParseRange(rawValue, out var min, out var max) && actual >= min && actual <= max,
            "in" => rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Any(v => decimal.TryParse(v, out var n) && n == actual),
            _ => false
        };
    }

    private static bool ParseRange(string rawValue, out decimal min, out decimal max)
    {
        min = max = 0;
        var parts = rawValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2) return false;
        return decimal.TryParse(parts[0], out min) && decimal.TryParse(parts[1], out max);
    }
}