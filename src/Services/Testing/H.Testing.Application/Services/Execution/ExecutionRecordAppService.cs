using H.Testing.Application.Contracts;
using H.Testing.Application.Mapping;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试执行记录服务（数据库存储）
/// </summary>
public class ExecutionRecordAppService : ApplicationService, IExecutionRecordAppService
{
    private readonly IRepository<CaseRecordEntity, long> _repository;

    public ExecutionRecordAppService(IRepository<CaseRecordEntity, long> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<List<CaseExecutionRecordDto>>> GetByProjectIdAsync(long projectId)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(
            query.Where(r => r.ProjectId == projectId).OrderByDescending(r => r.StartTime));
        return BaseOutput<List<CaseExecutionRecordDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<List<CaseExecutionRecordDto>>> GetByTestCaseIdAsync(long projectId, long testCaseId)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(
            query.Where(r => r.ProjectId == projectId && r.CaseId == testCaseId)
                 .OrderByDescending(r => r.StartTime));
        return BaseOutput<List<CaseExecutionRecordDto>>.Ok(list.Select(e => e.ToDto()).ToList());
    }

    public async Task<BaseOutput<CaseExecutionRecordDto?>> GetByIdAsync(long projectId, long id)
    {
        var entity = await _repository.FindAsync(id);
        return BaseOutput<CaseExecutionRecordDto?>.Ok(entity != null && entity.ProjectId == projectId ? entity.ToDto() : null);
    }

    public async Task<BaseOutput<CaseExecutionRecordDto>> CreateAsync(CaseExecutionRecordDto record)
    {
        if (record.StartTime == default)
        {
            record.StartTime = DateTime.Now;
        }

        var entity = new CaseRecordEntity();
        record.Apply(entity);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return BaseOutput<CaseExecutionRecordDto>.Ok(entity.ToDto());
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long projectId, CaseExecutionRecordDto record)
    {
        if (record.Id == 0)
        {
            throw new UserFriendlyException("执行记录更新失败：记录尚未持久化（Id 为空）");
        }

        var entity = await _repository.FindAsync(record.Id);
        if (entity == null || entity.ProjectId != projectId)
        {
            return BaseOutput<bool>.Ok(false);
        }

        record.Apply(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(long projectId, long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null || entity.ProjectId != projectId)
        {
            return BaseOutput<bool>.Ok(false);
        }

        await _repository.DeleteAsync(entity, autoSave: true);
        return BaseOutput<bool>.Ok(true);
    }

    public async Task<BaseOutput> CleanupOldRecordsAsync(long projectId, int keepCount = 100)
    {
        var query = await _repository.GetQueryableAsync();
        var oldRecords = await AsyncExecuter.ToListAsync(
            query.Where(r => r.ProjectId == projectId)
                 .OrderByDescending(r => r.StartTime)
                 .Skip(keepCount));

        if (oldRecords.Count > 0)
        {
            await _repository.DeleteManyAsync(oldRecords, autoSave: true);
        }
        return BaseOutput.Ok();
    }

    public async Task<BaseOutput<ExecutionStatistics>> GetStatisticsAsync(long projectId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = await _repository.GetQueryableAsync();
        query = query.Where(r => r.ProjectId == projectId);

        if (startDate.HasValue)
        {
            query = query.Where(r => r.StartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.StartTime <= endDate.Value);
        }

        var records = await AsyncExecuter.ToListAsync(query);

        return BaseOutput<ExecutionStatistics>.Ok(new ExecutionStatistics
        {
            TotalExecutions = records.Count,
            SuccessExecutions = records.Count(r => r.Status == (int)ExecutionStatus.Success),
            FailedExecutions = records.Count(r => r.Status == (int)ExecutionStatus.Failed),
            CancelledExecutions = records.Count(r => r.Status == (int)ExecutionStatus.Cancelled),
            AverageDuration = records.Count > 0 ? records.Average(r => r.Duration) : 0,
            TotalDuration = records.Sum(r => r.Duration)
        });
    }
}
