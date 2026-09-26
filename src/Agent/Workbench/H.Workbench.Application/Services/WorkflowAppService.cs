using H.Abp.Application.Contracts;
using H.Util.Base;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using System.Text.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Workbench.Application;

/// <summary>
/// 员工工作流管理服务实现
/// </summary>
public class WorkflowAppService : ApplicationService, IWorkflowAppService
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IRepository<WorkflowEntity, Guid> _workflowRepository;

    public WorkflowAppService(IRepository<WorkflowEntity, Guid> workflowRepository)
    {
        _workflowRepository = workflowRepository;
    }

    public async Task<BaseOutput<PagedResultDto<WorkflowDto>>> GetListAsync(WorkflowQueryDto input)
    {
        var query = await _workflowRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.AgentType))
        {
            query = query.Where(x => x.AgentType == input.AgentType);
        }

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.WorkflowName.Contains(input.Filter) || x.Description.Contains(input.Filter));
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new(new PagedResultDto<WorkflowDto>(totalCount, entities.Select(MapToDto).ToList()));
    }

    public async Task<BaseOutput<WorkflowDto>> GetAsync(Guid id)
    {
        var entity = await _workflowRepository.GetAsync(id);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<WorkflowDto>> CreateAsync(CreateWorkflowDto input)
    {
        var steps = NormalizeSteps(input.Steps);
        await EnsureNameUniqueAsync(input.AgentType, input.WorkflowName, null);

        var entity = await _workflowRepository.InsertAsync(new WorkflowEntity
        {
            AgentType = input.AgentType.Trim(),
            WorkflowName = input.WorkflowName.Trim(),
            Description = input.Description?.Trim() ?? string.Empty,
            Steps = steps,
            IsEnabled = input.IsEnabled
        });

        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<WorkflowDto>> UpdateAsync(Guid id, UpdateWorkflowDto input)
    {
        var steps = NormalizeSteps(input.Steps);

        var entity = await _workflowRepository.GetAsync(id);
        await EnsureNameUniqueAsync(entity.AgentType, input.WorkflowName, id);

        entity.WorkflowName = input.WorkflowName.Trim();
        entity.Description = input.Description?.Trim() ?? string.Empty;
        entity.Steps = steps;
        entity.IsEnabled = input.IsEnabled;

        return new(MapToDto(await _workflowRepository.UpdateAsync(entity)));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await _workflowRepository.DeleteAsync(id);
        return new();
    }

    /// <summary>
    /// 校验并序列化步骤：步骤名与提示词必填，缺省的稳定标识在此补齐
    /// </summary>
    private static string NormalizeSteps(List<WorkflowStepDto>? steps)
    {
        var list = steps ?? [];
        if (list.Count == 0)
        {
            throw new InvalidOperationException("工作流至少需要一个步骤");
        }

        for (var i = 0; i < list.Count; i++)
        {
            var step = list[i];
            if (string.IsNullOrWhiteSpace(step.Name) || string.IsNullOrWhiteSpace(step.Prompt))
            {
                throw new InvalidOperationException($"第 {i + 1} 步的名称与任务提示词都不能为空");
            }

            step.Name = step.Name.Trim();
            step.Prompt = step.Prompt.Trim();
            step.Id = string.IsNullOrWhiteSpace(step.Id) ? Guid.NewGuid().ToString("N")[..8] : step.Id;
            step.AgentType = string.IsNullOrWhiteSpace(step.AgentType) ? null : step.AgentType.Trim();
            step.OnFailure = step.OnFailure == "Skip" ? "Skip" : "Stop";
        }

        return JsonSerializer.Serialize(list, WriteOptions);
    }

    private async Task EnsureNameUniqueAsync(string agentType, string workflowName, Guid? excludeId)
    {
        var query = await _workflowRepository.GetQueryableAsync();
        var name = workflowName.Trim();
        var type = agentType.Trim();

        if (await AsyncExecuter.AnyAsync(query.Where(x =>
                x.AgentType == type && x.WorkflowName == name && (excludeId == null || x.Id != excludeId))))
        {
            throw new InvalidOperationException($"工作流名称 '{name}' 在该员工下已存在");
        }
    }

    private static WorkflowDto MapToDto(WorkflowEntity entity) => new()
    {
        Id = entity.Id,
        AgentType = entity.AgentType,
        WorkflowName = entity.WorkflowName,
        Description = entity.Description,
        IsEnabled = entity.IsEnabled,
        Steps = ParseSteps(entity.Steps),
        CreationTime = entity.CreationTime,
        CreatorId = entity.CreatorId,
        LastModificationTime = entity.LastModificationTime,
        LastModifierId = entity.LastModifierId
    };

    private static List<WorkflowStepDto> ParseSteps(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<WorkflowStepDto>>(json, ReadOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
