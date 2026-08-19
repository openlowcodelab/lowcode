using H.LowCode.MetaSchema;
using System.Text.RegularExpressions;

namespace H.LowCode.Application.Contracts;

/// <summary>
/// 字段校验逻辑（纯逻辑实现，服务端与客户端均可本地调用，无需远程请求）
/// </summary>
public static class FieldValidator
{
    /// <summary>
    /// 按规则列表校验字段值（按启用状态与优先级顺序执行，遇到首个失败即返回）
    /// </summary>
    public static ValidationResult Validate(object? value, IList<ValidationRuleSchema>? validationRules)
    {
        if (validationRules == null || validationRules.Count == 0)
            return ValidationResult.Success();

        var sortedRules = validationRules
            .Where(r => r.IsEnabled)
            .OrderBy(r => r.Order)
            .ToList();

        foreach (var rule in sortedRules)
        {
            var result = ValidateSingleRule(value, rule);
            if (!result.IsValid)
                return result;
        }

        return ValidationResult.Success();
    }

    private static ValidationResult ValidateSingleRule(object? value, ValidationRuleSchema rule)
    {
        try
        {
            switch (rule.RuleType)
            {
                case ValidationRuleTypeEnum.Required:
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return ValidationResult.Failure(rule.ErrorMessage ?? "此字段为必填项", ValidationRuleTypeEnum.Required);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.MinLength:
                    if (value == null) return ValidationResult.Success();
                    if ((value.ToString()?.Length ?? 0) < rule.MinLength)
                        return ValidationResult.Failure(rule.ErrorMessage ?? $"最少需要 {rule.MinLength} 个字符", ValidationRuleTypeEnum.MinLength);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.MaxLength:
                    if (value == null) return ValidationResult.Success();
                    if ((value.ToString()?.Length ?? 0) > rule.MaxLength)
                        return ValidationResult.Failure(rule.ErrorMessage ?? $"最多允许 {rule.MaxLength} 个字符", ValidationRuleTypeEnum.MaxLength);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.MinValue:
                    if (value == null) return ValidationResult.Success();
                    if (decimal.TryParse(value.ToString(), out var minValue) && minValue < rule.MinValue)
                        return ValidationResult.Failure(rule.ErrorMessage ?? $"值不能小于 {rule.MinValue}", ValidationRuleTypeEnum.MinValue);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.MaxValue:
                    if (value == null) return ValidationResult.Success();
                    if (decimal.TryParse(value.ToString(), out var maxValue) && maxValue > rule.MaxValue)
                        return ValidationResult.Failure(rule.ErrorMessage ?? $"值不能大于 {rule.MaxValue}", ValidationRuleTypeEnum.MaxValue);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.Pattern:
                    if (value == null || string.IsNullOrEmpty(rule.Pattern)) return ValidationResult.Success();
                    if (!new Regex(rule.Pattern).IsMatch(value.ToString() ?? ""))
                        return ValidationResult.Failure(rule.ErrorMessage ?? "格式不正确", ValidationRuleTypeEnum.Pattern);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.Email:
                    if (value == null) return ValidationResult.Success();
                    if (!new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").IsMatch(value.ToString() ?? ""))
                        return ValidationResult.Failure(rule.ErrorMessage ?? "请输入有效的邮箱地址", ValidationRuleTypeEnum.Email);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.Phone:
                    if (value == null) return ValidationResult.Success();
                    if (!new Regex(@"^1[3-9]\d{9}$").IsMatch(value.ToString() ?? ""))
                        return ValidationResult.Failure(rule.ErrorMessage ?? "请输入有效的手机号码", ValidationRuleTypeEnum.Phone);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.Url:
                    if (value == null) return ValidationResult.Success();
                    if (!Uri.TryCreate(value.ToString(), UriKind.Absolute, out _))
                        return ValidationResult.Failure(rule.ErrorMessage ?? "请输入有效的URL地址", ValidationRuleTypeEnum.Url);
                    return ValidationResult.Success();

                case ValidationRuleTypeEnum.Custom:
                default:
                    return ValidationResult.Success();
            }
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure($"校验规则执行错误: {ex.Message}", rule.RuleType);
        }
    }
}
