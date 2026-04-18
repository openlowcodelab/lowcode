using Volo.Abp.Application.Services;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批实例服务接口
/// </summary>
public interface IApprovalInstanceService : IApplicationService
{
    /// <summary>
    /// 启动审批实例
    /// </summary>
    Task<ApprovalInstanceDto> StartAsync(StartApprovalInstanceDto input);
    
    /// <summary>
    /// 获取我发起的审批实例
    /// </summary>
    Task<List<ApprovalInstanceDto>> GetMyApprovalsAsync(string userId);
    
    /// <summary>
    /// 获取审批实例详情
    /// </summary>
    Task<ApprovalInstanceDto> GetByIdAsync(string id);
    
    /// <summary>
    /// 取消审批实例
    /// </summary>
    Task CancelAsync(string id);
}
