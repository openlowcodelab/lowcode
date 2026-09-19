using AutoMapper;
using H.Abp.Application.Contracts;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
using System.Linq.Dynamic.Core;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace H.Workbench.Application;

/// <summary>
/// 任务执行日志查询应用服务
/// </summary>
public class TaskLogAppService : ApplicationService, ITaskLogAppService
{
    private readonly IRepository<TaskLogEntity, Guid> _logRepository;
    private readonly IRepository<TaskEntity, Guid> _taskRepository;
    private readonly IRepository<TaskExecutionStepEntity, Guid> _stepRepository;
    private readonly IRepository<ArtifactEntity, Guid> _artifactRepository;
    private readonly IMapper _objectMapper;

    public TaskLogAppService(
        IRepository<TaskLogEntity, Guid> logRepository,
        IRepository<TaskEntity, Guid> taskRepository,
        IRepository<TaskExecutionStepEntity, Guid> stepRepository,
        IRepository<ArtifactEntity, Guid> artifactRepository,
        IMapper objectMapper)
    {
        _logRepository = logRepository;
        _taskRepository = taskRepository;
        _stepRepository = stepRepository;
        _artifactRepository = artifactRepository;
        _objectMapper = objectMapper;
    }

    public async Task<BaseOutput<PagedResultDto<TaskLogDto>>> GetListAsync(TaskLogQueryDto input)
    {
        var queryable = await _logRepository.GetQueryableAsync();

        var query = queryable.AsQueryable();

        if (input.TaskId.HasValue)
        {
            query = query.Where(l => l.TaskId == input.TaskId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            query = query.Where(l => l.Status == input.Status);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        query = query.OrderByDescending(l => l.StartTime);

        var logs = await AsyncExecuter.ToListAsync(
            query
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        // 获取所有关联的任务名称
        var taskIds = logs.Select(l => l.TaskId).Distinct().ToList();
        var tasks = new List<TaskEntity>();
        foreach (var taskId in taskIds)
        {
            var task = await _taskRepository.FindAsync(taskId);
            if (task != null)
            {
                tasks.Add(task);
            }
        }

        var taskNameMap = tasks.ToDictionary(t => t.Id, t => t.TaskName);

        var dtos = logs
            .Select(l =>
            {
                var dto = _objectMapper.Map<TaskLogEntity, TaskLogDto>(l);
                if (taskNameMap.TryGetValue(l.TaskId, out var taskName))
                {
                    dto.TaskName = taskName;
                }
                return dto;
            })
            .ToList();

        return new(new PagedResultDto<TaskLogDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<TaskLogDto>> GetAsync(Guid id)
    {
        var log = await _logRepository.FindAsync(id);
        if (log == null)
        {
            throw new EntityNotFoundException(typeof(TaskLogEntity), id);
        }

        var dto = _objectMapper.Map<TaskLogEntity, TaskLogDto>(log);

        var task = await _taskRepository.FindAsync(log.TaskId);
        if (task != null)
        {
            dto.TaskName = task.TaskName;
        }

        return new(dto);
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        // 无 FK，级联清子表防孤儿行
        var stepQueryable = await _stepRepository.GetQueryableAsync();
        var steps = await AsyncExecuter.ToListAsync(stepQueryable.Where(s => s.TaskLogId == id));
        if (steps.Count > 0)
        {
            await _stepRepository.DeleteManyAsync(steps, autoSave: true);
        }

        var artifactQueryable = await _artifactRepository.GetQueryableAsync();
        var artifacts = await AsyncExecuter.ToListAsync(artifactQueryable.Where(a => a.TaskLogId == id));
        if (artifacts.Count > 0)
        {
            await _artifactRepository.DeleteManyAsync(artifacts, autoSave: true);
        }

        await _logRepository.DeleteAsync(id);
        return new();
    }
}
