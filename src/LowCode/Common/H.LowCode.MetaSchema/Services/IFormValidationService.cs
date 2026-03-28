using H.LowCode.MetaSchema;

namespace H.LowCode.MetaSchema.Services;

/// <summary>
/// 表单校验服务接口
/// </summary>
public interface IFormValidationService
{
    /// <summary>
    /// 校验单个字段值
    /// </summary>
    /// <param name="value">字段值</param>
    /// <param name="validationRules">校验规则</param>
    /// <returns>校验结果</returns>
    ValidationResult ValidateField(object? value, IList<ValidationRuleSchema>? validationRules);

    /// <summary>
    /// 校验表单数据
    /// </summary>
    /// <param name="formData">表单数据</param>
    /// <param name="components">组件列表</param>
    /// <returns>校验结果</returns>
    FormValidationResult ValidateForm(Dictionary<string, object?> formData, IList<ComponentSchemaBase> components);

    /// <summary>
    /// 获取字段的校验规则
    /// </summary>
    /// <param name="componentId">组件ID</param>
    /// <param name="components">组件列表</param>
    /// <returns>校验规则</returns>
    IList<ValidationRuleSchema>? GetValidationRules(string componentId, IList<ComponentSchemaBase> components);
}

/// <summary>
/// 校验结果
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public ValidationRuleTypeEnum? FailedRuleType { get; set; }

    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(string errorMessage, ValidationRuleTypeEnum? ruleType = null) => 
        new() { IsValid = false, ErrorMessage = errorMessage, FailedRuleType = ruleType };
}

/// <summary>
/// 表单校验结果
/// </summary>
public class FormValidationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, ValidationResult> FieldResults { get; set; } = new();
    public List<string> ErrorMessages { get; set; } = new();

    public void AddFieldResult(string fieldName, ValidationResult result)
    {
        FieldResults[fieldName] = result;
        if (!result.IsValid && !string.IsNullOrEmpty(result.ErrorMessage))
        {
            ErrorMessages.Add($"{fieldName}: {result.ErrorMessage}");
        }
    }
}