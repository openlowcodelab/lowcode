using H.LowCode.Application.Contracts;
using H.LowCode.MetaSchema;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.Application;

/// <summary>
/// 表单校验服务实现（委托本地校验逻辑 FieldValidator）
/// </summary>
[RemoteService]
public class FormValidationAppService : ApplicationService, IFormValidationAppService
{
    public Task<BaseOutput<ValidationResult>> ValidateFieldAsync(FieldValidationInput input)
    {
        var result = FieldValidator.Validate(input?.Value, input?.ValidationRules);
        return Task.FromResult(new BaseOutput<ValidationResult>(result));
    }

    public Task<BaseOutput<IList<ValidationRuleSchema>>> GetValidationRulesAsync(string componentId, IList<ComponentSchemaBase> components)
    {
        var component = components.FirstOrDefault(c => c.Id == componentId);
        return Task.FromResult(new BaseOutput<IList<ValidationRuleSchema>>(component?.ValidationRules));
    }
}
