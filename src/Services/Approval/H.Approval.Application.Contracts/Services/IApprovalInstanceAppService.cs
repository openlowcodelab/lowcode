using Volo.Abp.Application.Services;

namespace H.Approval.Application.Contracts;

/// <summary>
/// 审批实例服务接口
/// </summary>
public interface IApprovalInstanceAppService : IApplicationService
{
    /// <summary>
    /// 启动审批实例
    /// </summary>
    Task<ApprovalInstanceDto> StartAsync(StartApprovalInstanceDto input);

    /// <summary>
    /// 获取我发起的审批实例(当前登录用户)
    /// </summary>
    Task<List<ApprovalInstanceDto>> GetMyApprovalsAsync();

    /// <summary>
    /// 获取审批实例详情(含任务历史)
    /// </summary>
    Task<ApprovalInstanceDto> GetByIdAsync(string id);

    /// <summary>
    /// 取消审批实例
    /// </summary>
    Task CancelAsync(string id);
}
