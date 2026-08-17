using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批实例服务接口
/// </summary>
public interface IApprovalInstanceAppService : IAppService
{
    /// <summary>
    /// 启动审批实例
    /// </summary>
    Task<BaseOutput<ApprovalInstanceDto>> StartAsync(StartApprovalInstanceDto input);

    /// <summary>
    /// 获取我发起的审批实例(当前登录用户)
    /// </summary>
    Task<BaseOutput<List<ApprovalInstanceDto>>> GetMyApprovalsAsync();

    /// <summary>
    /// 获取审批实例详情(含任务历史)
    /// </summary>
    Task<BaseOutput<ApprovalInstanceDto>> GetByIdAsync(string id);

    /// <summary>
    /// 取消审批实例
    /// </summary>
    Task<BaseOutput> CancelAsync(string id);
}
