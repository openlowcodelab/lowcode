using H.LowCode.Application.Contracts;
using H.LowCode.MetaSchema;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.Application;

/// <summary>
/// 表单校验服务实现
/// </summary>
[RemoteService]
public class FormValidationAppService : ApplicationService, IFormValidationAppService
{
    public ValidationResult ValidateField(object? value, IList<ValidationRuleSchema>? validationRules)
    {
        if (validationRules == null || !validationRules.Any())
            return ValidationResult.Success();

        // 按优先级排序校验规则
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

    public FormValidationResult ValidateForm(Dictionary<string, object?> formData, IList<ComponentSchemaBase> components)
    {
        var result = new FormValidationResult { IsValid = true };

        foreach (var component in components)
        {
            if (component.ValidationRules == null || !component.ValidationRules.Any())
                continue;

            var fieldName = component.Name ?? component.Id;
            var fieldValue = formData.ContainsKey(fieldName) ? formData[fieldName] : null;

            var validationResult = ValidateField(fieldValue, component.ValidationRules);
            result.AddFieldResult(fieldName, validationResult);

            if (!validationResult.IsValid)
                result.IsValid = false;
        }

        return result;
    }

    public IList<ValidationRuleSchema>? GetValidationRules(string componentId, IList<ComponentSchemaBase> components)
    {
        var component = components.FirstOrDefault(c => c.Id == componentId);
        return component?.ValidationRules;
    }

    private ValidationResult ValidateSingleRule(object? value, ValidationRuleSchema rule)
    {
        try
        {
            switch (rule.RuleType)
            {
                case ValidationRuleTypeEnum.Required:
                    return ValidateRequired(value, rule);

                case ValidationRuleTypeEnum.MinLength:
                    return ValidateMinLength(value, rule);

                case ValidationRuleTypeEnum.MaxLength:
                    return ValidateMaxLength(value, rule);

                case ValidationRuleTypeEnum.MinValue:
                    return ValidateMinValue(value, rule);

                case ValidationRuleTypeEnum.MaxValue:
                    return ValidateMaxValue(value, rule);

                case ValidationRuleTypeEnum.Pattern:
                    return ValidatePattern(value, rule);

                case ValidationRuleTypeEnum.Email:
                    return ValidateEmail(value, rule);

                case ValidationRuleTypeEnum.Phone:
                    return ValidatePhone(value, rule);

                case ValidationRuleTypeEnum.Url:
                    return ValidateUrl(value, rule);

                case ValidationRuleTypeEnum.Custom:
                    return ValidateCustom(value, rule);

                default:
                    return ValidationResult.Success();
            }
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure($"校验规则执行错误: {ex.Message}", rule.RuleType);
        }
    }

    private ValidationResult ValidateRequired(object? value, ValidationRuleSchema rule)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Failure(
                rule.ErrorMessage ?? "此字段为必填项",
                ValidationRuleTypeEnum.Required);
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidateMinLength(object? value, ValidationRuleSchema rule)
    {
        if (value == null) return ValidationResult.Success();

        var stringValue = value.ToString();
        if (stringValue != null && stringValue.Length < rule.MinLength)
        {
            return ValidationResult.Failure(
                rule.ErrorMessage ?? $"最少需要 {rule.MinLength} 个字符",
                ValidationRuleTypeEnum.MinLength);
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidateMaxLength(object? value, ValidationRuleSchema rule)
    {
        if (value == null) return ValidationResult.Success();

        var stringValue = value.ToString();
        if (stringValue != null && stringValue.Length > rule.MaxLength)
        {
            return ValidationResult.Failure(
                rule.ErrorMessage ?? $"最多允许 {rule.MaxLength} 个字符",
                ValidationRuleTypeEnum.MaxLength);
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidateMinValue(object? value, ValidationRuleSchema rule)
    {
        if (value == null) return ValidationResult.Success();

        if (decimal.TryParse(value.ToString(), out var numericValue))
        {
            if (numericValue < rule.MinValue)
            {
                return ValidationResult.Failure(
                    rule.ErrorMessage ?? $"值不能小于 {rule.MinValue}",
                    ValidationRuleTypeEnum.MinValue);
            }
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidateMaxValue(object? value, ValidationRuleSchema rule)
    {
        if (value == null) return ValidationResult.Success();

        if (decimal.TryParse(value.ToString(), out var numericValue))
        {
            if (numericValue > rule.MaxValue)
            {
                return ValidationResult.Failure(
                    rule.ErrorMessage ?? $"值不能大于 {rule.MaxValue}",
                    ValidationRuleTypeEnum.MaxValue);
            }
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidatePattern(object? value, ValidationRuleSchema rule)
    {
        if (value == null) return ValidationResult.Success();

        if (!string.IsNullOrEmpty(rule.Pattern))
        {
            var regex = new Regex(rule.Pattern);
            if (!regex.IsMatch(value.ToString() ?? ""))
            {
                return ValidationResult.Failure(
                    rule.ErrorMessage ?? "格式不正确",
                    ValidationRuleTypeEnum.Pattern);
            }
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidateEmail(object? value, ValidationRuleSchema rule)
    {
        if (value == null) return ValidationResult.Success();

        var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        var regex = new Regex(emailPattern);
        if (!regex.IsMatch(value.ToString() ?? ""))
        {
            return ValidationResult.Failure(
                rule.ErrorMessage ?? "请输入有效的邮箱地址",
                ValidationRuleTypeEnum.Email);
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidatePhone(object? value, ValidationRuleSchema rule)
    {
        if (value == null) return ValidationResult.Success();

        var phonePattern = @"^1[3-9]\d{9}$";
        var regex = new Regex(phonePattern);
        if (!regex.IsMatch(value.ToString() ?? ""))
        {
            return ValidationResult.Failure(
                rule.ErrorMessage ?? "请输入有效的手机号码",
                ValidationRuleTypeEnum.Phone);
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidateUrl(object? value, ValidationRuleSchema rule)
    {
        if (value == null) return ValidationResult.Success();

        if (!Uri.TryCreate(value.ToString(), UriKind.Absolute, out _))
        {
            return ValidationResult.Failure(
                rule.ErrorMessage ?? "请输入有效的URL地址",
                ValidationRuleTypeEnum.Url);
        }
        return ValidationResult.Success();
    }

    private ValidationResult ValidateCustom(object? value, ValidationRuleSchema rule)
    {
        // 自定义校验规则可以通过Expression属性来实现
        // 这里可以扩展支持JavaScript表达式或其他自定义逻辑
        if (!string.IsNullOrEmpty(rule.Expression))
        {
            // TODO: 实现自定义表达式校验
            // 可以使用JavaScript引擎或其他表达式解析器
        }
        return ValidationResult.Success();
    }
}