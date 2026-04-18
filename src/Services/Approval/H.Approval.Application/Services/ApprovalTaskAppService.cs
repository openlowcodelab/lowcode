using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;

namespace H.Approval.Application.Services;

/// <summary>
/// 审批任务应用服务
/// </summary>
public class ApprovalTaskAppService : ApplicationService, IApprovalTaskAppService
{
    private readonly ILogger<ApprovalTaskAppService> _logger;
    private readonly IApprovalTaskRepository _taskRepository;

    public ApprovalTaskAppService(
        ILogger<ApprovalTaskAppService> logger,
        IApprovalTaskRepository taskRepository)
    {
        _logger = logger;
        _taskRepository = taskRepository;
    }
    
    public async Task<List<ApprovalTaskDto>> GetPendingTasksAsync(string userId)
    {
        _logger.LogInformation("获取待审批任务: UserId={UserId}", userId);
        
        var entities = await _taskRepository.GetPendingByAssigneeIdAsync(userId);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<ApprovalTaskDto>> GetCompletedTasksAsync(string userId)
    {
        _logger.LogInformation("获取已审批任务: UserId={UserId}", userId);
        
        var entities = await _taskRepository.GetCompletedByAssigneeIdAsync(userId);
        return entities.Select(MapToDto).ToList();
    }

    public async Task ApproveAsync(ApprovalTaskActionDto input)
    {
        _logger.LogInformation("审批任务: TaskId={TaskId}, Action={Action}", 
            input.TaskId, input.Action == 1 ? "通过" : "驳回");
        
        var entity = await _taskRepository.GetByIdAsync(input.TaskId);
        if (entity == null)
        {
            throw new KeyNotFoundException($"审批任务不存在: {input.TaskId}");
        }
        
        // 1. 更新任务状态
        entity.Status = input.Action;
        entity.Comment = input.Comment;
        entity.ApprovalTime = DateTime.Now;
        
        await _taskRepository.UpdateAsync(entity);
        
        // 2. 恢复Elsa工作流
        try
        {
            var approved = input.Action == 1;
            
            // TODO: 实际调用Elsa恢复工作流
            // await _workflowRuntime.ResumeWorkflowAsync(
            //     task.InstanceId,
            //     new Dictionary<string, object>
            //     {
            //         { "ApprovalResult", approved },
            //         { "Comment", input.Comment }
            //     });
            
            _logger.LogInformation("任务已审批: TaskId={TaskId}, Result={Result}", 
                input.TaskId, approved ? "通过" : "驳回");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "审批任务失败: TaskId={TaskId}", input.TaskId);
            throw;
        }
    }
    
    /// <summary>
    /// 创建审批任务(内部方法,由工作流活动调用)
    /// </summary>
    public async Task<string> CreateTaskAsync(string instanceId, string assigneeId, string assigneeName, string nodeName, string approvalName, string instanceTitle)
    {
        var taskId = Guid.NewGuid().ToString();
        var entity = new ApprovalTask(taskId)
        {
            InstanceId = instanceId,
            ApprovalName = approvalName,
            InstanceTitle = instanceTitle,
            NodeName = nodeName,
            AssigneeId = assigneeId,
            AssigneeName = assigneeName,
            Status = 0, // 待审批
            CreationTime = DateTime.Now
        };
        
        await _taskRepository.InsertAsync(entity);
        
        return entity.Id;
    }
    
    private static ApprovalTaskDto MapToDto(ApprovalTask entity)
    {
        return new ApprovalTaskDto
        {
            Id = entity.Id,
            InstanceId = entity.InstanceId,
            ApprovalName = entity.ApprovalName,
            InstanceTitle = entity.InstanceTitle,
            NodeName = entity.NodeName,
            AssigneeId = entity.AssigneeId,
            AssigneeName = entity.AssigneeName,
            Status = entity.Status,
            CreationTime = entity.CreationTime,
            ApprovalTime = entity.ApprovalTime,
            Comment = entity.Comment
        };
    }
}
