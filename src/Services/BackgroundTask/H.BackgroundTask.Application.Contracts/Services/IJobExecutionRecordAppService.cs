using H.Abp.Application.Contracts;

namespace H.BackgroundTask.Application.Contracts;

/// <summary>
/// 任务执行记录查询接口
/// </summary>
public interface IJobExecutionRecordAppService : IAppService
{
    /// <summary>分页查询执行记录</summary>
    Task<PagedResultDto<JobCaseExecutionRecordDto>> GetListAsync(JobExecutionRecordQueryDto input);

    /// <summary>获取单条执行记录</summary>
    Task<JobCaseExecutionRecordDto> GetAsync(Guid id);
}
