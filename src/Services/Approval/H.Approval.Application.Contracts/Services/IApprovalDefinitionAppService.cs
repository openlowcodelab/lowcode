using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批定义服务接口
/// </summary>
public interface IApprovalDefinitionAppService : IAppService
{
    /// <summary>
    /// 获取所有审批定义
    /// </summary>
    Task<BaseOutput<List<ApprovalDefinitionDto>>> GetAllAsync();

    /// <summary>
    /// 根据 ID 获取审批定义
    /// </summary>
    Task<BaseOutput<ApprovalDefinitionDto>> GetByIdAsync(string id);

    /// <summary>
    /// 创建审批定义
    /// </summary>
    Task<BaseOutput<ApprovalDefinitionDto>> CreateAsync(CreateApprovalDefinitionDto input);

    /// <summary>
    /// 更新审批定义
    /// </summary>
    Task<BaseOutput<ApprovalDefinitionDto>> UpdateAsync(UpdateApprovalDefinitionDto input);

    /// <summary>
    /// 删除审批定义
    /// </summary>
    Task<BaseOutput> DeleteAsync(string id);

    /// <summary>
    /// 启用/禁用审批定义
    /// </summary>
    Task<BaseOutput> ToggleEnabledAsync(string id, bool enabled);

    /// <summary>
    /// 获取预置审批模板(用于"从模板创建")
    /// </summary>
    Task<BaseOutput<List<ApprovalTemplateDto>>> GetTemplatesAsync();
}
