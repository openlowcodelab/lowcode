using H.Abp.Application.Contracts;
using H.LowCode.MetaSchema;
using H.Util.Base;

namespace H.LowCode.Application.Contracts;

/// <summary>
/// 表单校验服务接口
/// </summary>
public interface IFormValidationAppService : IAppService
{
    /// <summary>
    /// 校验单个字段值
    /// </summary>
    Task<BaseOutput<ValidationResult>> ValidateFieldAsync(FieldValidationInput input);

    /// <summary>
    /// 获取字段的校验规则
    /// </summary>
    /// <param name="componentId">组件ID</param>
    /// <param name="components">组件列表</param>
    /// <returns>校验规则</returns>
    Task<BaseOutput<IList<ValidationRuleSchema>>> GetValidationRulesAsync(string componentId, IList<ComponentSchemaBase> components);
}

/// <summary>
/// 字段校验入参
/// </summary>
public class FieldValidationInput
{
    /// <summary>
    /// 字段值
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// 校验规则
    /// </summary>
    public IList<ValidationRuleSchema>? ValidationRules { get; set; }
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