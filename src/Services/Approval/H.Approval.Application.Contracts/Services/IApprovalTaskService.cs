using Volo.Abp.Application.Services;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批任务服务接口
/// </summary>
public interface IApprovalTaskService : IApplicationService
{
    /// <summary>
    /// 获取待我审批的任务列表
    /// </summary>
    Task<List<ApprovalTaskDto>> GetPendingTasksAsync(string userId);
    
    /// <summary>
    /// 获取我已审批的任务列表
    /// </summary>
    Task<List<ApprovalTaskDto>> GetCompletedTasksAsync(string userId);
    
    /// <summary>
    /// 审批任务
    /// </summary>
    Task ApproveAsync(ApprovalTaskActionDto input);
}
