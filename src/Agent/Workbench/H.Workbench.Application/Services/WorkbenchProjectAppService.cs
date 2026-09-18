using H.Abp.Application.Contracts;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
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
    private readonly IRepository<TaskEntity, Guid> _taskRepository;

    public WorkbenchProjectAppService(
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<TaskEntity, Guid> taskRepository)
    {
        _projectRepository = projectRepository;
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

        var taskQuery = await _taskRepository.GetQueryableAsync();
        var ids = entities.Select(x => x.Id).ToList();
        var taskCounts = await AsyncExecuter.ToListAsync(
            taskQuery.Where(t => t.ProjectId != null && ids.Contains(t.ProjectId.Value))
                .GroupBy(t => t.ProjectId!.Value)
                .Select(g => new { ProjectId = g.Key, Count = g.Count() }));

        var dtos = entities.Select(x => new WorkbenchProjectDto
        {
            Id = x.Id,
            ProjectName = x.ProjectName,
            Description = x.Description,
            TaskCount = taskCounts.FirstOrDefault(c => c.ProjectId == x.Id)?.Count ?? 0,
            CreationTime = x.CreationTime,
            CreatorId = x.CreatorId,
            LastModificationTime = x.LastModificationTime,
            LastModifierId = x.LastModifierId
        }).ToList();

        return new(new PagedResultDto<WorkbenchProjectDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<WorkbenchProjectDto>> GetAsync(Guid id)
    {
        var entity = await _projectRepository.FindAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(ProjectEntity), id);
        }

        return new(new WorkbenchProjectDto
        {
            Id = entity.Id,
            ProjectName = entity.ProjectName,
            Description = entity.Description,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        });
    }

    public async Task<BaseOutput<WorkbenchProjectDto>> CreateAsync(CreateWorkbenchProjectDto input)
    {
        var entity = new ProjectEntity
        {
            ProjectName = input.ProjectName,
            Description = input.Description
        };

        entity = await _projectRepository.InsertAsync(entity);
        return new(new WorkbenchProjectDto
        {
            Id = entity.Id,
            ProjectName = entity.ProjectName,
            Description = entity.Description,
            CreationTime = entity.CreationTime
        });
    }

    public async Task<BaseOutput<WorkbenchProjectDto>> UpdateAsync(Guid id, UpdateWorkbenchProjectDto input)
    {
        var entity = await _projectRepository.GetAsync(id);
        entity.ProjectName = input.ProjectName;
        entity.Description = input.Description;

        entity = await _projectRepository.UpdateAsync(entity);
        return new(new WorkbenchProjectDto
        {
            Id = entity.Id,
            ProjectName = entity.ProjectName,
            Description = entity.Description,
            CreationTime = entity.CreationTime
        });
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        // 解除任务的项目归属后删除项目
        var taskQuery = await _taskRepository.GetQueryableAsync();
        var tasks = await AsyncExecuter.ToListAsync(taskQuery.Where(t => t.ProjectId == id));
        foreach (var task in tasks)
        {
            task.ProjectId = null;
            await _taskRepository.UpdateAsync(task);
        }

        await _projectRepository.DeleteAsync(id);
        return new();
    }
}
