using H.BackgroundTask.Application.Contracts;
using H.BackgroundTask.Application.Mapping;
using H.BackgroundTask.EntityFrameworkCore;
using Volo.Abp;
using H.Abstractions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.BackgroundTask.Application.Services;

/// <summary>
/// 后台任务应用服务：任务的增删改查，并与 Hangfire 调度联动。
/// </summary>
public class BackgroundJobAppService : ApplicationService, IBackgroundJobAppService
{
    private readonly IRepository<BackgroundJobEntity, Guid> _repository;
    private readonly IJobScheduler _scheduler;

    public BackgroundJobAppService(
        IRepository<BackgroundJobEntity, Guid> repository,
        IJobScheduler scheduler)
    {
        _repository = repository;
        _scheduler = scheduler;
    }

    public async Task<PagedResultDto<BackgroundJobDto>> GetListAsync(BackgroundJobQueryDto input)
    {
        var query = await _repository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
            query = query.Where(x => x.Name.Contains(input.Filter));
        if (input.TriggerKind.HasValue)
            query = query.Where(x => x.TriggerKind == (int)input.TriggerKind!.Value);
        if (input.ExecuteType.HasValue)
            query = query.Where(x => x.ExecuteType == (int)input.ExecuteType!.Value);
        if (input.IsEnabled.HasValue)
            query = query.Where(x => x.IsEnabled == input.IsEnabled!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(e => e.ToDto()).ToList();
        return new PagedResultDto<BackgroundJobDto>(totalCount, dtos);
    }

    public async Task<BackgroundJobDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return entity.ToDto();
    }

    public async Task<BackgroundJobDto> CreateAsync(CreateBackgroundJobDto input)
    {
        ValidateInput(input.TriggerKind, input.ExecuteType, input.CronExpression, input.ApiUrl, input.SqlConnectionString, input.SqlStatement);

        var entity = input.ToEntity();
        await _repository.InsertAsync(entity, autoSave: true);

        // 注册调度并回写 Hangfire 作业标识
        entity.HangfireJobId = _scheduler.Schedule(entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return entity.ToDto();
    }

    public async Task<BackgroundJobDto> UpdateAsync(Guid id, UpdateBackgroundJobDto input)
    {
        ValidateInput(input.TriggerKind, input.ExecuteType, input.CronExpression, input.ApiUrl, input.SqlConnectionString, input.SqlStatement);

        var entity = await _repository.GetAsync(id);

        // 先按旧定义移除既有调度，再按新定义注册
        _scheduler.Remove(entity);

        input.Apply(entity);
        entity.HangfireJobId = _scheduler.Schedule(entity);
        await _repository.UpdateAsync(entity, autoSave: true);

        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        _scheduler.Remove(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    public async Task EnableAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        entity.IsEnabled = true;
        entity.HangfireJobId = _scheduler.Schedule(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
    }

    public async Task DisableAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        _scheduler.Remove(entity);
        entity.IsEnabled = false;
        entity.HangfireJobId = null;
        await _repository.UpdateAsync(entity, autoSave: true);
    }

    public async Task TriggerAsync(Guid id)
    {
        // 确认任务存在
        await _repository.GetAsync(id);
        _scheduler.Trigger(id);
    }

    private static void ValidateInput(
        JobTriggerKind triggerKind,
        JobExecuteType executeType,
        string? cronExpression,
        string? apiUrl,
        string? sqlConnectionString,
        string? sqlStatement)
    {
        if (triggerKind == JobTriggerKind.Recurring && string.IsNullOrWhiteSpace(cronExpression))
            throw new UserFriendlyException("周期任务必须配置 Cron 表达式");

        if (executeType == JobExecuteType.Api && string.IsNullOrWhiteSpace(apiUrl))
            throw new UserFriendlyException("API 任务必须配置 API 地址");

        if (executeType == JobExecuteType.Sql &&
            (string.IsNullOrWhiteSpace(sqlConnectionString) || string.IsNullOrWhiteSpace(sqlStatement)))
            throw new UserFriendlyException("SQL 任务必须配置数据源与 SQL 语句");
    }
}
