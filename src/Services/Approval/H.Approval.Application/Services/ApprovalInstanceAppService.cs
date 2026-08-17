using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Volo.Abp.Application.Services;

namespace H.Approval.Application;

/// <summary>
/// 审批实例应用服务
/// </summary>
public class ApprovalInstanceAppService : ApplicationService, IApprovalInstanceAppService
{
    private readonly ILogger<ApprovalInstanceAppService> _logger;
    private readonly IApprovalDefinitionAppService _definitionService;
    private readonly IApprovalInstanceRepository _instanceRepository;
    private readonly IApprovalTaskRepository _taskRepository;
    private readonly ApprovalWorkflowEngine _workflowEngine;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApprovalInstanceAppService(
        ILogger<ApprovalInstanceAppService> logger,
        IApprovalDefinitionAppService definitionService,
        IApprovalInstanceRepository instanceRepository,
        IApprovalTaskRepository taskRepository,
        ApprovalWorkflowEngine workflowEngine,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _definitionService = definitionService;
        _instanceRepository = instanceRepository;
        _taskRepository = taskRepository;
        _workflowEngine = workflowEngine;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BaseOutput<ApprovalInstanceDto>> StartAsync(StartApprovalInstanceDto input)
    {
        _logger.LogInformation("启动审批实例: DefinitionId={DefinitionId}, Title={Title}",
            input.DefinitionId, input.Title);

        // 1. 获取审批定义
        var definition = (await _definitionService.GetByIdAsync(input.DefinitionId)).Data!;

        // 2. 解析节点树
        var root = _workflowEngine.ParseDefinition(definition.DefinitionJson);
        if (root == null)
        {
            throw new InvalidOperationException("审批流程定义为空,无法启动");
        }

        var (creatorId, creatorName) = GetCurrentUser();
        var variables = _workflowEngine.ParseVariables(input.VariablesJson);

        // 3. 创建审批实例
        var instanceId = Guid.NewGuid().ToString();
        var entity = new ApprovalInstance(instanceId)
        {
            DefinitionId = input.DefinitionId,
            DefinitionName = definition.Name,
            Title = input.Title,
            Status = ApprovalStatusEnum.Running,
            CreatorId = creatorId,
            CreatorName = creatorName,
            VariablesJson = input.VariablesJson,
            CreationTime = DateTime.Now
        };

        // 4. 定位首个审批节点
        var first = _workflowEngine.GetFirstApprove(root, creatorId, creatorName, variables);
        if (first == null)
        {
            entity.Status = ApprovalStatusEnum.Completed;
            entity.CurrentNodeName = "无审批节点";
            entity.CompletionTime = DateTime.Now;
            await _instanceRepository.InsertAsync(entity);
            _logger.LogInformation("审批定义无审批节点,实例直接完成: InstanceId={InstanceId}", entity.Id);
            return BaseOutput<ApprovalInstanceDto>.Ok(MapToDto(entity));
        }

        var (node, assignees) = first.Value;
        entity.CurrentNodeId = node.Id;
        entity.CurrentNodeName = node.NodeName;
        await _instanceRepository.InsertAsync(entity);

        // 5. 根据审批模式创建任务
        await CreateTasksForNodeAsync(entity, node, assignees, definition.Name, input.Title);

        _logger.LogInformation("审批实例已启动: InstanceId={InstanceId}, 首节点={NodeName}, 审批模式={Mode}, 审批人数={Count}",
            entity.Id, node.NodeName, node.ApproverMode, assignees.Count);

        return BaseOutput<ApprovalInstanceDto>.Ok(MapToDto(entity));
    }

    /// <summary>
    /// 根据审批模式为节点创建任务
    /// </summary>
    internal async Task CreateTasksForNodeAsync(
        ApprovalInstance instance, ApproveModel node,
        List<(string Id, string Name)> assignees, string approvalName, string instanceTitle)
    {
        switch (node.ApproverMode)
        {
            case ApproverModeEnum.Sequential:
                // 依次审批: 只创建第一个审批人的任务
                if (assignees.Count > 0)
                {
                    var first = assignees[0];
                    await CreateTaskAsync(instance.Id, node.Id, node.NodeName,
                        first.Id, first.Name, approvalName, instanceTitle);
                }
                break;

            case ApproverModeEnum.CounterSign:
            case ApproverModeEnum.OrSign:
                // 会签/或签: 为所有审批人创建任务
                foreach (var assignee in assignees)
                {
                    await CreateTaskAsync(instance.Id, node.Id, node.NodeName,
                        assignee.Id, assignee.Name, approvalName, instanceTitle);
                }
                break;
        }
    }

    /// <summary>
    /// 创建单个审批任务
    /// </summary>
    internal async Task CreateTaskAsync(
        string instanceId, string nodeId, string nodeName,
        string assigneeId, string assigneeName, string approvalName, string instanceTitle)
    {
        var task = new ApprovalTask(Guid.NewGuid().ToString())
        {
            InstanceId = instanceId,
            NodeId = nodeId,
            ApprovalName = approvalName,
            InstanceTitle = instanceTitle,
            NodeName = nodeName,
            AssigneeId = assigneeId,
            AssigneeName = assigneeName,
            Status = 0,
            CreationTime = DateTime.Now
        };
        await _taskRepository.InsertAsync(task);
    }

    public async Task<BaseOutput<List<ApprovalInstanceDto>>> GetMyApprovalsAsync()
    {
        var (userId, _) = GetCurrentUser();
        _logger.LogInformation("获取我发起的审批: UserId={UserId}", userId);

        var entities = await _instanceRepository.GetByCreatorIdAsync(userId);
        return BaseOutput<List<ApprovalInstanceDto>>.Ok(entities.Select(MapToDto).ToList());
    }

    public async Task<BaseOutput<ApprovalInstanceDto>> GetByIdAsync(string id)
    {
        _logger.LogInformation("获取审批实例详情: Id={Id}", id);

        var entity = await _instanceRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"审批实例不存在: {id}");
        }

        var dto = MapToDto(entity);
        var tasks = await _taskRepository.GetByInstanceIdAsync(id);
        dto.Tasks = tasks.Select(MapTaskToDto).ToList();
        return BaseOutput<ApprovalInstanceDto>.Ok(dto);
    }

    public async Task<BaseOutput> CancelAsync(string id)
    {
        _logger.LogInformation("取消审批实例: Id={Id}", id);

        var entity = await _instanceRepository.GetByIdAsync(id);
        if (entity == null)
        {
            return BaseOutput.Ok();
        }

        entity.Status = ApprovalStatusEnum.Cancelled;
        entity.CompletionTime = DateTime.Now;
        await _instanceRepository.UpdateAsync(entity);

        // 取消所有待审批任务
        var tasks = await _taskRepository.GetByInstanceIdAsync(id);
        foreach (var task in tasks.Where(t => t.Status == 0))
        {
            task.Status = 3; // 已取消
            task.Comment = "审批实例已取消";
            task.ApprovalTime = DateTime.Now;
            await _taskRepository.UpdateAsync(task);
        }

        _logger.LogInformation("审批实例已取消: Id={Id}", id);

        return BaseOutput.Ok();
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

    private static ApprovalInstanceDto MapToDto(ApprovalInstance entity)
    {
        return new ApprovalInstanceDto
        {
            Id = entity.Id,
            DefinitionId = entity.DefinitionId,
            DefinitionName = entity.DefinitionName,
            Title = entity.Title,
            Status = entity.Status,
            CreatorId = entity.CreatorId,
            CreatorName = entity.CreatorName,
            CurrentNodeId = entity.CurrentNodeId,
            CurrentNodeName = entity.CurrentNodeName,
            VariablesJson = entity.VariablesJson,
            CreationTime = entity.CreationTime,
            CompletionTime = entity.CompletionTime
        };
    }

    private static ApprovalTaskDto MapTaskToDto(ApprovalTask entity)
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
