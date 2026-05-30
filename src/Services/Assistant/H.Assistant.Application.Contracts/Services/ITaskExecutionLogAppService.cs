using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 任务执行日志查询服务接口
/// </summary>
public interface ITaskExecutionLogAppService : IApplicationService
{
    /// <summary>
    /// 获取执行日志列表（分页）
    /// </summary>
    Task<PagedResultDto<TaskExecutionLogDto>> GetListAsync(TaskExecutionLogQueryDto input);

    /// <summary>
    /// 获取单个执行日志
    /// </summary>
    Task<TaskExecutionLogDto> GetAsync(Guid id);

    /// <summary>
    /// 删除执行日志
    /// </summary>
    Task DeleteAsync(Guid id);
}
