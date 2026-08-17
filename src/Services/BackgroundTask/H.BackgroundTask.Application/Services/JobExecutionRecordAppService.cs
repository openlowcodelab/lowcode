using H.Abp.Application.Contracts;
using H.BackgroundTask.Application.Contracts;
using H.BackgroundTask.Application.Mapping;
using H.BackgroundTask.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.BackgroundTask.Application.Services;

/// <summary>
/// 任务执行记录查询服务。
/// </summary>
public class JobExecutionRecordAppService : ApplicationService, IJobExecutionRecordAppService
{
    private readonly IRepository<JobExecutionRecordEntity, Guid> _repository;

    public JobExecutionRecordAppService(IRepository<JobExecutionRecordEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<PagedResultDto<JobCaseExecutionRecordDto>>> GetListAsync(JobExecutionRecordQueryDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (input.JobId.HasValue)
            query = query.Where(x => x.JobId == input.JobId!.Value);
        if (input.Status.HasValue)
            query = query.Where(x => x.Status == (int)input.Status!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.StartTime).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return BaseOutput<PagedResultDto<JobCaseExecutionRecordDto>>.Ok(new PagedResultDto<JobCaseExecutionRecordDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<JobCaseExecutionRecordDto>> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return BaseOutput<JobCaseExecutionRecordDto>.Ok(entity.ToDto());
    }
}
