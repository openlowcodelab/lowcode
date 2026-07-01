using System.Linq.Dynamic.Core;
using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;

namespace H.Assistant.Application;

/// <summary>
/// 任务执行日志查询应用服务
/// </summary>
public class TaskLogAppService : ApplicationService, ITaskLogAppService
{
    private readonly IRepository<TaskLogEntity, Guid> _logRepository;
    private readonly IRepository<TaskEntity, Guid> _taskRepository;
    private readonly IMapper _objectMapper;

    public TaskLogAppService(
        IRepository<TaskLogEntity, Guid> logRepository,
        IRepository<TaskEntity, Guid> taskRepository,
        IMapper objectMapper)
    {
        _logRepository = logRepository;
        _taskRepository = taskRepository;
        _objectMapper = objectMapper;
    }

    public async Task<PagedResultDto<TaskLogDto>> GetListAsync(TaskLogQueryDto input)
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

        return new PagedResultDto<TaskLogDto>(totalCount, dtos);
    }

    public async Task<TaskLogDto> GetAsync(Guid id)
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

        return dto;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _logRepository.DeleteAsync(id);
    }
}
