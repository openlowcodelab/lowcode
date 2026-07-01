using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;

namespace H.Approval.Application.Services;

/// <summary>
/// 审批实例应用服务
/// </summary>
public class ApprovalInstanceAppService : ApplicationService, IApprovalInstanceAppService
{
    private readonly ILogger<ApprovalInstanceAppService> _logger;
    private readonly IApprovalDefinitionAppService _definitionService;
    private readonly IApprovalInstanceRepository _instanceRepository;

    public ApprovalInstanceAppService(
        ILogger<ApprovalInstanceAppService> logger,
        IApprovalDefinitionAppService definitionService,
        IApprovalInstanceRepository instanceRepository)
    {
        _logger = logger;
        _definitionService = definitionService;
        _instanceRepository = instanceRepository;
    }
    
    public async Task<ApprovalInstanceDto> StartAsync(StartApprovalInstanceDto input)
    {
        _logger.LogInformation("启动审批实例: DefinitionId={DefinitionId}, Title={Title}", 
            input.DefinitionId, input.Title);
        
        // 1. 获取审批定义
        var definition = await _definitionService.GetByIdAsync(input.DefinitionId);
        
        // 2. 创建审批实例
        var instanceId = Guid.NewGuid().ToString();
        var entity = new ApprovalInstance(instanceId)
        {
            DefinitionId = input.DefinitionId,
            DefinitionName = definition.Name,
            Status = ApprovalStatusEnum.Running,
            CreatorId = "current-user-id", // TODO: 从ABP CurrentUser获取
            CreatorName = "当前用户",
            CurrentNodeName = "发起",
            CreationTime = DateTime.Now
        };
        
        await _instanceRepository.InsertAsync(entity);
        
        // 3. 创建第一个审批任务
        var taskId = Guid.NewGuid().ToString();
        var firstTask = new ApprovalTask(taskId)
        {
            InstanceId = entity.Id,
            ApprovalName = definition.Name,
            InstanceTitle = input.Title,
            NodeName = "发起",
            AssigneeId = entity.CreatorId,
            AssigneeName = entity.CreatorName,
            Status = 0, // 待审批
            CreationTime = DateTime.Now
        };
        
        // TODO: 实际调用Elsa启动工作流和创建任务
        
        _logger.LogInformation("审批实例已启动: InstanceId={InstanceId}", entity.Id);
        
        return MapToDto(entity);
    }

    public async Task<List<ApprovalInstanceDto>> GetMyApprovalsAsync(string userId)
    {
        _logger.LogInformation("获取我发起的审批: UserId={UserId}", userId);
        
        var entities = await _instanceRepository.GetByCreatorIdAsync(userId);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<ApprovalInstanceDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("获取审批实例详情: Id={Id}", id);
        
        var entity = await _instanceRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"审批实例不存在: {id}");
        }
        
        return MapToDto(entity);
    }

    public async Task CancelAsync(string id)
    {
        _logger.LogInformation("取消审批实例: Id={Id}", id);
        
        var entity = await _instanceRepository.GetByIdAsync(id);
        if (entity != null)
        {
            entity.Status = ApprovalStatusEnum.Cancelled;
            entity.CompletionTime = DateTime.Now;
            await _instanceRepository.UpdateAsync(entity);
            
            // TODO: 取消Elsa工作流
            // await _workflowRuntime.CancelWorkflowAsync(id);
            
            _logger.LogInformation("审批实例已取消: Id={Id}", id);
        }
    }
    
    private static ApprovalInstanceDto MapToDto(ApprovalInstance entity)
    {
        return new ApprovalInstanceDto
        {
            Id = entity.Id,
            DefinitionId = entity.DefinitionId,
            DefinitionName = entity.DefinitionName,
            Status = entity.Status,
            CreatorId = entity.CreatorId,
            CreatorName = entity.CreatorName,
            CurrentNodeName = entity.CurrentNodeName,
            CreationTime = entity.CreationTime,
            CompletionTime = entity.CompletionTime
        };
    }
}
