using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H.Approval.Application.Contracts;
using Volo.Abp.Application.Services;

namespace H.Approval.Application.Services;

/// <summary>
/// 审批任务应用服务
/// </summary>
public class ApprovalTaskAppService : ApplicationService, IApprovalTaskService
{
    // TODO: 注入 Elsa 相关服务
    
    public async Task<List<ApprovalTaskDto>> GetPendingTasksAsync(string userId)
    {
        // TODO: 实现获取待我审批的任务列表
        return await Task.FromResult(new List<ApprovalTaskDto>());
    }

    public async Task<List<ApprovalTaskDto>> GetCompletedTasksAsync(string userId)
    {
        // TODO: 实现获取我已审批的任务列表
        return await Task.FromResult(new List<ApprovalTaskDto>());
    }

    public async Task ApproveAsync(ApprovalTaskActionDto input)
    {
        // TODO: 实现审批任务
        await Task.CompletedTask;
    }
}
