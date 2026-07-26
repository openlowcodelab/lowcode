using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 定时任务管理服务接口
/// </summary>
public interface ITaskAppService : IAppService
{
    /// <summary>
    /// 获取任务列表（分页）
    /// </summary>
    Task<PagedResultDto<TaskDto>> GetListAsync(TaskQueryDto input);

    /// <summary>
    /// 获取单个任务
    /// </summary>
    Task<TaskDto> GetAsync(Guid id);

    /// <summary>
    /// 创建任务
    /// </summary>
    Task<TaskDto> CreateAsync(CreateTaskDto input);

    /// <summary>
    /// 更新任务
    /// </summary>
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskDto input);

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
    Task<List<TaskLogDto>> GetExecutionLogsAsync(Guid taskId, int maxResultCount = 10);
}
