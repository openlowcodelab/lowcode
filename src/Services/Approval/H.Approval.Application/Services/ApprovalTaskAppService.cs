using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Volo.Abp.Application.Services;

namespace H.Approval.Application;

/// <summary>
/// 审批任务应用服务
/// </summary>
public class ApprovalTaskAppService : ApplicationService, IApprovalTaskAppService
{
    private readonly ILogger<ApprovalTaskAppService> _logger;
    private readonly IApprovalTaskRepository _taskRepository;
    private readonly IApprovalInstanceRepository _instanceRepository;
    private readonly IApprovalDefinitionAppService _definitionService;
    private readonly ApprovalWorkflowEngine _workflowEngine;
    private readonly ApprovalInstanceAppService _instanceService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApprovalTaskAppService(
        ILogger<ApprovalTaskAppService> logger,
        IApprovalTaskRepository taskRepository,
        IApprovalInstanceRepository instanceRepository,
        IApprovalDefinitionAppService definitionService,
        ApprovalWorkflowEngine workflowEngine,
        ApprovalInstanceAppService instanceService,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _taskRepository = taskRepository;
        _instanceRepository = instanceRepository;
        _definitionService = definitionService;
        _workflowEngine = workflowEngine;
        _instanceService = instanceService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BaseOutput<List<ApprovalTaskDto>>> GetPendingTasksAsync()
    {
        var (userId, _) = GetCurrentUser();
        _logger.LogInformation("获取待审批任务: UserId={UserId}", userId);

        var entities = await _taskRepository.GetPendingByAssigneeIdAsync(userId);
        return BaseOutput<List<ApprovalTaskDto>>.Ok(entities.Select(MapToDto).ToList());
    }

    public async Task<BaseOutput<List<ApprovalTaskDto>>> GetCompletedTasksAsync()
    {
        var (userId, _) = GetCurrentUser();
        _logger.LogInformation("获取已审批任务: UserId={UserId}", userId);

        var entities = await _taskRepository.GetCompletedByAssigneeIdAsync(userId);
        return BaseOutput<List<ApprovalTaskDto>>.Ok(entities.Select(MapToDto).ToList());
    }

    public async Task<BaseOutput<List<ApprovalTaskDto>>> GetByInstanceIdAsync(string instanceId)
    {
        _logger.LogInformation("获取实例任务历史: InstanceId={InstanceId}", instanceId);
        var entities = await _taskRepository.GetByInstanceIdAsync(instanceId);
        return BaseOutput<List<ApprovalTaskDto>>.Ok(entities.Select(MapToDto).ToList());
    }

    public async Task<BaseOutput> ApproveAsync(ApprovalTaskActionDto input)
    {
        var approved = input.Action == 1;
        _logger.LogInformation("审批任务: TaskId={TaskId}, Action={Action}",
            input.TaskId, approved ? "通过" : "驳回");

        var entity = await _taskRepository.GetByIdAsync(input.TaskId);
        if (entity == null)
        {
            throw new KeyNotFoundException($"审批任务不存在: {input.TaskId}");
        }

        if (entity.Status != 0)
        {
            throw new InvalidOperationException($"审批任务已处理,不能重复操作: {input.TaskId}");
        }

        // 1. 更新任务状态
        entity.Status = input.Action; // 1=通过, 2=驳回
        entity.Comment = input.Comment;
        entity.ApprovalTime = DateTime.Now;
        await _taskRepository.UpdateAsync(entity);

        // 2. 推进工作流
        var instance = await _instanceRepository.GetByIdAsync(entity.InstanceId);
        if (instance == null)
        {
            _logger.LogWarning("审批任务对应的实例不存在: InstanceId={InstanceId}", entity.InstanceId);
            return BaseOutput.Ok();
        }

        if (!approved)
        {
            // 驳回: 取消同节点其他待处理任务,实例结束
            await CancelSiblingPendingTasks(entity.InstanceId, entity.NodeId, entity.Id);
            instance.Status = ApprovalStatusEnum.Rejected;
            instance.CompletionTime = DateTime.Now;
            instance.CurrentNodeName = $"{entity.NodeName}(已驳回)";
            await _instanceRepository.UpdateAsync(instance);
            _logger.LogInformation("审批已驳回,实例结束: InstanceId={InstanceId}", instance.Id);
            return BaseOutput.Ok();
        }

        // 通过: 需要判断审批模式
        var definition = (await _definitionService.GetByIdAsync(instance.DefinitionId)).Data!;
        var root = _workflowEngine.ParseDefinition(definition.DefinitionJson);

        if (root == null || string.IsNullOrEmpty(instance.CurrentNodeId))
        {
            instance.Status = ApprovalStatusEnum.Completed;
            instance.CompletionTime = DateTime.Now;
            await _instanceRepository.UpdateAsync(instance);
            _logger.LogInformation("无后续流程节点,实例完成: InstanceId={InstanceId}", instance.Id);
            return BaseOutput.Ok();
        }

        // 查找当前审批节点
        var approveNodes = _workflowEngine.GetApproveNodesOnPath(
            root, _workflowEngine.ParseVariables(instance.VariablesJson));
        var currentNode = approveNodes.FirstOrDefault(n => n.Id == entity.NodeId);

        if (currentNode != null)
        {
            // 根据审批模式处理
            var shouldAdvance = await HandleApprovalMode(
                currentNode, entity, instance, root, definition.Name);

            if (!shouldAdvance)
            {
                // 会签: 还有其他审批人未处理,等待
                _logger.LogInformation("会签模式: 等待其他审批人处理: NodeId={NodeId}", entity.NodeId);
                return BaseOutput.Ok();
            }
        }

        // 流转到下一节点
        await AdvanceToNextNode(instance, entity, root, definition.Name);

        return BaseOutput.Ok();
    }

    /// <summary>
    /// 根据审批模式处理,返回是否应该流转到下一节点
    /// </summary>
    private async Task<bool> HandleApprovalMode(
        ApproveModel currentNode, ApprovalTask task,
        ApprovalInstance instance, NodeModelBase root, string approvalName)
    {
        switch (currentNode.ApproverMode)
        {
            case ApproverModeEnum.Sequential:
                // 依次审批: 创建下一个审批人的任务
                var assignees = _workflowEngine.ResolveAssignees(
                    currentNode, instance.CreatorId, instance.CreatorName);
                var currentIndex = assignees.FindIndex(a => a.Id == task.AssigneeId);
                if (currentIndex >= 0 && currentIndex < assignees.Count - 1)
                {
                    // 还有下一个审批人
                    var next = assignees[currentIndex + 1];
                    await _instanceService.CreateTaskAsync(
                        instance.Id, currentNode.Id, currentNode.NodeName,
                        next.Id, next.Name, approvalName, instance.Title);
                    _logger.LogInformation("依次审批: 流转到下一个审批人: {AssigneeName}", next.Name);
                    return false; // 不流转到下一节点,继续当前节点
                }
                // 当前节点最后一个审批人,流转到下一节点
                return true;

            case ApproverModeEnum.CounterSign:
                // 会签: 检查是否所有人都已通过
                var nodeTasks = await _taskRepository.GetByNodeIdAsync(instance.Id, currentNode.Id);
                var pendingCount = nodeTasks.Count(t => t.Status == 0);
                if (pendingCount > 0)
                {
                    // 还有未处理的任务,等待
                    return false;
                }
                // 所有人都已处理,流转到下一节点
                _logger.LogInformation("会签完成: 所有审批人已通过,流转到下一节点");
                return true;

            case ApproverModeEnum.OrSign:
                // 或签: 其中一人通过即可,取消其他待处理任务
                await CancelSiblingPendingTasks(instance.Id, currentNode.Id, task.Id);
                _logger.LogInformation("或签: 一人通过,取消其他待处理任务");
                return true;

            default:
                return true;
        }
    }

    /// <summary>
    /// 取消同节点其他待处理任务
    /// </summary>
    private async Task CancelSiblingPendingTasks(string instanceId, string nodeId, string excludeTaskId)
    {
        var nodeTasks = await _taskRepository.GetByNodeIdAsync(instanceId, nodeId);
        var toCancel = nodeTasks.Where(t => t.Id != excludeTaskId && t.Status == 0).ToList();
        foreach (var t in toCancel)
        {
            t.Status = 3; // 已取消
            t.Comment = "已被其他审批人处理";
            t.ApprovalTime = DateTime.Now;
        }
        if (toCancel.Count > 0)
        {
            await _taskRepository.UpdateRangeAsync(toCancel);
        }
    }

    /// <summary>
    /// 流转到下一审批节点
    /// </summary>
    private async Task AdvanceToNextNode(
        ApprovalInstance instance, ApprovalTask currentTask,
        NodeModelBase root, string approvalName)
    {
        var variables = _workflowEngine.ParseVariables(instance.VariablesJson);
        var next = _workflowEngine.GetNextApprove(
            root, instance.CurrentNodeId!, instance.CreatorId, instance.CreatorName, variables);

        if (next == null)
        {
            // 流程结束
            instance.Status = ApprovalStatusEnum.Completed;
            instance.CompletionTime = DateTime.Now;
            instance.CurrentNodeId = null;
            instance.CurrentNodeName = "流程结束";
            await _instanceRepository.UpdateAsync(instance);
            _logger.LogInformation("审批流程已完成: InstanceId={InstanceId}", instance.Id);
            return;
        }

        var (node, assignees) = next.Value;

        // 创建下一节点的任务
        await _instanceService.CreateTasksForNodeAsync(
            instance, node, assignees, approvalName, instance.Title);

        instance.CurrentNodeId = node.Id;
        instance.CurrentNodeName = node.NodeName;
        await _instanceRepository.UpdateAsync(instance);

        _logger.LogInformation("任务已通过,流转到下一节点: InstanceId={InstanceId}, NextNode={NodeName}, 审批模式={Mode}",
            instance.Id, node.NodeName, node.ApproverMode);
    }

    private (string Id, string Name) GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier);
        var nameClaim = user?.FindFirst(ClaimTypes.Name);
        var id = idClaim?.Value ?? "anonymous";
        var name = nameClaim?.Value ?? "匿名用户";
        return (id, name);
    }

    private static ApprovalTaskDto MapToDto(ApprovalTask entity)
    {
        return new ApprovalTaskDto
        {
            Id = entity.Id,
            InstanceId = entity.InstanceId,
            ApprovalName = entity.ApprovalName,
            InstanceTitle = entity.InstanceTitle,
            NodeId = entity.NodeId,
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
