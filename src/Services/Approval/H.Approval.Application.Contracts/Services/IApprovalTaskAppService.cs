using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批任务服务接口
/// </summary>
public interface IApprovalTaskAppService : IAppService
{
    /// <summary>
    /// 获取待我审批的任务列表(当前登录用户)
    /// </summary>
    Task<BaseOutput<List<ApprovalTaskDto>>> GetPendingTasksAsync();

    /// <summary>
    /// 获取我已审批的任务列表(当前登录用户)
    /// </summary>
    Task<BaseOutput<List<ApprovalTaskDto>>> GetCompletedTasksAsync();

    /// <summary>
    /// 获取某审批实例的全部任务(任务历史)
    /// </summary>
    Task<BaseOutput<List<ApprovalTaskDto>>> GetByInstanceIdAsync(string instanceId);

    /// <summary>
    /// 审批任务(通过/驳回),并推进工作流
    /// </summary>
    Task<BaseOutput> ApproveAsync(ApprovalTaskActionDto input);
}
