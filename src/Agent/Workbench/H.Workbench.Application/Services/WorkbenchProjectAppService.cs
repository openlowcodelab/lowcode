using H.Abp.Application.Contracts;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
using System.Text.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace H.Workbench.Application;

/// <summary>
/// 项目管理服务实现
/// </summary>
public class WorkbenchProjectAppService : ApplicationService, IWorkbenchProjectAppService
{
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectResourceEntity, Guid> _resourceRepository;
    private readonly IRepository<TaskEntity, Guid> _taskRepository;

    public WorkbenchProjectAppService(
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<ProjectResourceEntity, Guid> resourceRepository,
        IRepository<TaskEntity, Guid> taskRepository)
    {
        _projectRepository = projectRepository;
        _resourceRepository = resourceRepository;
        _taskRepository = taskRepository;
    }

    public async Task<BaseOutput<PagedResultDto<WorkbenchProjectDto>>> GetListAsync(WorkbenchProjectQueryDto input)
    {
        var query = await _projectRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.ProjectName.Contains(input.Filter) || x.Description.Contains(input.Filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var ids = entities.Select(x => x.Id).ToList();
        var resourcesByProject = await GetResourcesByProjectIdsAsync(ids);

        var taskQuery = await _taskRepository.GetQueryableAsync();
        var taskCounts = await AsyncExecuter.ToListAsync(
            taskQuery.Where(t => t.ProjectId != null && ids.Contains(t.ProjectId.Value))
                .GroupBy(t => t.ProjectId!.Value)
                .Select(g => new { ProjectId = g.Key, Count = g.Count() }));

        var dtos = entities.Select(x => new WorkbenchProjectDto
        {
            Id = x.Id,
            ProjectName = x.ProjectName,
            Description = x.Description,
            Resources = resourcesByProject.GetValueOrDefault(x.Id) ?? [],
            TaskCount = taskCounts.FirstOrDefault(c => c.ProjectId == x.Id)?.Count ?? 0,
            CreationTime = x.CreationTime,
            CreatorId = x.CreatorId,
            LastModificationTime = x.LastModificationTime,
            LastModifierId = x.LastModifierId
        }).ToList();

        return new(new PagedResultDto<WorkbenchProjectDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<List<WorkbenchProjectDto>>> GetByIdsAsync(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0) return new(new List<WorkbenchProjectDto>());

        var query = await _projectRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => ids.Contains(x.Id)));
        var resourcesByProject = await GetResourcesByProjectIdsAsync(entities.Select(x => x.Id).ToList());

        return new(entities.Select(x => new WorkbenchProjectDto
        {
            Id = x.Id,
            ProjectName = x.ProjectName,
            Description = x.Description,
            Resources = resourcesByProject.GetValueOrDefault(x.Id) ?? []
        }).ToList());
    }

    public async Task<BaseOutput<WorkbenchProjectDto>> GetAsync(Guid id)
    {
        var entity = await _projectRepository.FindAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ProjectEntity), id);
        }

        return new(await ToDtoAsync(entity));
    }

    public async Task<BaseOutput<WorkbenchProjectDto>> CreateAsync(CreateWorkbenchProjectDto input)
    {
        var entity = new ProjectEntity
        {
            ProjectName = input.ProjectName,
            Description = input.Description
        };

        entity = await _projectRepository.InsertAsync(entity);
        await ReplaceResourcesAsync(entity.Id, input.Resources);

        return new(await ToDtoAsync(entity));
    }

    public async Task<BaseOutput<WorkbenchProjectDto>> UpdateAsync(Guid id, UpdateWorkbenchProjectDto input)
    {
        var entity = await _projectRepository.GetAsync(id);
        entity.ProjectName = input.ProjectName;
        entity.Description = input.Description;

        entity = await _projectRepository.UpdateAsync(entity);
        await ReplaceResourcesAsync(id, input.Resources);

        return new(await ToDtoAsync(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        // 解除任务的项目归属后删除项目及其资源
        var taskQuery = await _taskRepository.GetQueryableAsync();
        var tasks = await AsyncExecuter.ToListAsync(taskQuery.Where(t => t.ProjectId == id));
        foreach (var task in tasks)
        {
            task.ProjectId = null;
            await _taskRepository.UpdateAsync(task);
        }

        await _resourceRepository.DeleteAsync(x => x.ProjectId == id);
        await _projectRepository.DeleteAsync(id);
        return new();
    }

    private async Task<WorkbenchProjectDto> ToDtoAsync(ProjectEntity entity)
    {
        var resourcesByProject = await GetResourcesByProjectIdsAsync([entity.Id]);
        return new WorkbenchProjectDto
        {
            Id = entity.Id,
            ProjectName = entity.ProjectName,
            Description = entity.Description,
            Resources = resourcesByProject.GetValueOrDefault(entity.Id) ?? [],
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    private async Task<Dictionary<Guid, List<WorkbenchProjectResourceDto>>> GetResourcesByProjectIdsAsync(List<Guid> projectIds)
    {
        if (projectIds.Count == 0) return [];

        var resourceQuery = await _resourceRepository.GetQueryableAsync();
        var resources = await AsyncExecuter.ToListAsync(
            resourceQuery.Where(x => projectIds.Contains(x.ProjectId)).OrderBy(x => x.CreationTime));

        return resources
            .GroupBy(x => x.ProjectId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(ToResourceDto).ToList());
    }

    private async Task ReplaceResourcesAsync(Guid projectId, List<WorkbenchProjectResourceInputDto>? inputs)
    {
        await _resourceRepository.DeleteAsync(x => x.ProjectId == projectId);

        foreach (var input in inputs ?? [])
        {
            var url = input.Url?.Trim();
            var resourceType = string.IsNullOrWhiteSpace(input.ResourceType) || !WorkbenchResourceTypes.All.Contains(input.ResourceType)
                ? WorkbenchResourceTypes.Other
                : input.ResourceType;

            await _resourceRepository.InsertAsync(new ProjectResourceEntity
            {
                ProjectId = projectId,
                ResourceType = resourceType,
                Name = input.Name.Trim(),
                Url = string.IsNullOrEmpty(url) ? null : url,
                Config = resourceType == WorkbenchResourceTypes.Code ? BuildCodeConfig(input.Branch) : null
            });
        }
    }

    private static WorkbenchProjectResourceDto ToResourceDto(ProjectResourceEntity x) => new()
    {
        ResourceType = x.ResourceType,
        Name = x.Name,
        Url = x.Url,
        Branch = ParseBranch(x.Config)
    };

    private static string? BuildCodeConfig(string? branch)
    {
        branch = branch?.Trim();
        if (string.IsNullOrEmpty(branch)) return null;
        return JsonSerializer.Serialize(new Dictionary<string, string> { ["branch"] = branch });
    }

    private static string? ParseBranch(string? config)
    {
        if (string.IsNullOrWhiteSpace(config)) return null;
        try
        {
            using var doc = JsonDocument.Parse(config);
            return doc.RootElement.TryGetProperty("branch", out var branch) ? branch.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
