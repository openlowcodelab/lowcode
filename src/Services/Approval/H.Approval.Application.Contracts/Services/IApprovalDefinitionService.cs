using Volo.Abp.Application.Services;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批定义服务接口
/// </summary>
public interface IApprovalDefinitionService : IApplicationService
{
    /// <summary>
    /// 获取所有审批定义
    /// </summary>
    Task<List<ApprovalDefinitionDto>> GetAllAsync();
    
    /// <summary>
    /// 根据 ID 获取审批定义
    /// </summary>
    Task<ApprovalDefinitionDto> GetByIdAsync(string id);
    
    /// <summary>
    /// 创建审批定义
    /// </summary>
    Task<ApprovalDefinitionDto> CreateAsync(CreateApprovalDefinitionDto input);
    
    /// <summary>
    /// 更新审批定义
    /// </summary>
    Task<ApprovalDefinitionDto> UpdateAsync(UpdateApprovalDefinitionDto input);
    
    /// <summary>
    /// 删除审批定义
    /// </summary>
    Task DeleteAsync(string id);
    
    /// <summary>
    /// 启用/禁用审批定义
    /// </summary>
    Task ToggleEnabledAsync(string id, bool enabled);
}
