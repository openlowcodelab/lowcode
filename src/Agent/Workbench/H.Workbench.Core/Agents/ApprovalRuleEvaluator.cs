using H.Workbench.Application.Contracts;
using System.Text.RegularExpressions;

namespace H.Workbench.Core.Agents;

/// <summary>
/// 审批规则求值：按 Priority 降序取第一条命中规则。
/// 规则只在"命中"时表态，未命中一律交回技能级 RequiresApproval 兜底——
/// 否则新增一条 Auto 规则就会意外放行所有未覆盖的工具。
/// </summary>
public sealed class ApprovalRuleEvaluator
{
    private sealed record Compiled(string ToolPattern, Regex? ToolRegex, string? ExactTool, Regex? ArgRegex, string Effect);

    private readonly List<Compiled> _rules;

    /// <summary>正则匹配超时，防止用户写的灾难性回溯正则把执行挂死</summary>
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(500);

    public ApprovalRuleEvaluator(IEnumerable<ApprovalRuleDto> rules)
    {
        _rules = rules
            .Where(r => r.IsEnabled && !string.IsNullOrWhiteSpace(r.ToolPattern))
            .OrderByDescending(r => r.Priority)
            .Select(r => new Compiled(
                r.ToolPattern,
                r.ToolPattern.Contains('*') ? BuildWildcard(r.ToolPattern) : null,
                r.ToolPattern.Contains('*') ? null : r.ToolPattern,
                string.IsNullOrWhiteSpace(r.ArgPattern) ? null : SafeRegex(r.ArgPattern),
                r.Effect))
            .ToList();
    }

    public bool HasRules => _rules.Count > 0;

    /// <summary>
    /// 返回 Require/Auto/Deny；null 表示无规则命中。
    /// </summary>
    public string? Evaluate(string toolName, string? argumentsJson)
    {
        foreach (var rule in _rules)
        {
            var toolHit = rule.ExactTool is not null
                ? string.Equals(rule.ExactTool, toolName, StringComparison.OrdinalIgnoreCase)
                : rule.ToolRegex?.IsMatch(toolName) == true;
            if (!toolHit) continue;

            if (rule.ArgRegex is not null)
            {
                if (string.IsNullOrEmpty(argumentsJson)) continue;
                bool argHit;
                try
                {
                    argHit = rule.ArgRegex.IsMatch(argumentsJson);
                }
                catch (RegexMatchTimeoutException)
                {
                    argHit = false;
                }
                if (!argHit) continue;
            }

            return rule.Effect;
        }

        return null;
    }

    private static Regex BuildWildcard(string pattern) =>
        new("^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexTimeout);

    private static Regex? SafeRegex(string pattern)
    {
        try
        {
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, RegexTimeout);
        }
        catch (ArgumentException)
        {
            // 建规则时已校验过；这里坏正则按"不命中"处理，绝不让工具调用因此失败
            return null;
        }
    }
}
