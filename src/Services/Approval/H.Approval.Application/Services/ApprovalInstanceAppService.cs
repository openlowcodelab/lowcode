using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using H.Approval.Application.Contracts;
using Volo.Abp.Application.Services;

namespace H.Approval.Application.Services;

/// <summary>
/// 审批实例应用服务
/// </summary>
public class ApprovalInstanceAppService : ApplicationService, IApprovalInstanceService
{
    // TODO: 注入 Elsa 的 IApprovalInstanceService
    
    public async Task<ApprovalInstanceDto> StartAsync(StartApprovalInstanceDto input)
    {
        // TODO: 实现启动审批实例
        return await Task.FromResult(new ApprovalInstanceDto
        {
            Id = Guid.NewGuid().ToString(),
            DefinitionId = input.DefinitionId,
            Status = ApprovalStatusEnum.Running,
            CreatorId = "current-user-id",
            CreatorName = "当前用户",
            CreationTime = DateTime.Now
        });
    }

    public async Task<List<ApprovalInstanceDto>> GetMyApprovalsAsync(string userId)
    {
        // TODO: 实现获取我发起的审批实例
        return await Task.FromResult(new List<ApprovalInstanceDto>());
    }

    public async Task<ApprovalInstanceDto> GetByIdAsync(string id)
    {
        // TODO: 实现获取审批实例详情
        return await Task.FromResult(new ApprovalInstanceDto { Id = id });
    }

    public async Task CancelAsync(string id)
    {
        // TODO: 实现取消审批实例
        await Task.CompletedTask;
    }
}
