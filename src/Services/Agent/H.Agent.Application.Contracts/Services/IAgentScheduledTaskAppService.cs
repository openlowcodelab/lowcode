using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.Agent.Application.Contracts;

/// <summary>
/// 定时任务管理服务接口
/// </summary>
public interface IAgentScheduledTaskAppService : IApplicationService
{
    /// <summary>
    /// 获取任务列表（分页）
    /// </summary>
    Task<PagedResultDto<ScheduledTaskDto>> GetListAsync(ScheduledTaskQueryDto input);

    /// <summary>
    /// 获取单个任务
    /// </summary>
    Task<ScheduledTaskDto> GetAsync(Guid id);

    /// <summary>
    /// 创建任务
    /// </summary>
    Task<ScheduledTaskDto> CreateAsync(CreateScheduledTaskInputDto input);

    /// <summary>
    /// 更新任务
    /// </summary>
    Task<ScheduledTaskDto> UpdateAsync(Guid id, UpdateScheduledTaskInputDto input);

    /// <summary>
    /// 删除任务
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// 启用/禁用任务
    /// </summary>
    Task ToggleEnableAsync(Guid id);

    /// <summary>
    /// 立即执行任务
    /// </summary>
    Task ExecuteNowAsync(Guid id);
    /// <summary>
    /// 执行单个任务（由后台Worker调用）
    /// </summary>
    Task ExecuteTaskAsync(Guid taskId);

    /// <summary>
    /// 获取任务的执行日志
    /// </summary>
    Task<List<TaskExecutionLogDto>> GetExecutionLogsAsync(Guid taskId, int maxResultCount = 10);
}
