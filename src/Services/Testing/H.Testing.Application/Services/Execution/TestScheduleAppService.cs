using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Hangfire;
using H.Util.Base;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 定时执行计划服务：计划增删改查，并与 Hangfire 调度联动
/// </summary>
public class TestScheduleAppService : ApplicationService, ITestScheduleAppService
{
    private const string RecurringJobPrefix = "testing-schedule:";

    private readonly IRepository<TestScheduleEntity, long> _repository;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public TestScheduleAppService(
        IRepository<TestScheduleEntity, long> repository,
        IRecurringJobManager recurringJobManager,
        IBackgroundJobClient backgroundJobClient)
    {
        _repository = repository;
        _recurringJobManager = recurringJobManager;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<BaseOutput<List<TestScheduleDto>>> GetByProjectIdAsync(long projectId)
    {
        var query = await _repository.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(
            query.Where(e => e.ProjectId == projectId).OrderBy(e => e.Name));
        return new(list.Select(ToDto).ToList());
    }

    public async Task<BaseOutput<long>> CreateAsync(TestScheduleDto dto)
    {
        ValidateSchedule(dto);

        var entity = new TestScheduleEntity { ProjectId = dto.ProjectId };
        ApplyDto(entity, dto);
        entity = await _repository.InsertAsync(entity, autoSave: true);

        SyncHangfireJob(entity);
        return new(entity.Id);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, TestScheduleDto dto)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new(false);
        }

        ValidateSchedule(dto);
        ApplyDto(entity, dto);
        await _repository.UpdateAsync(entity, autoSave: true);

        SyncHangfireJob(entity);
        return new(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new(false);
        }

        _recurringJobManager.RemoveIfExists(RecurringJobId(id));
        await _repository.DeleteAsync(entity, autoSave: true);
        return new(true);
    }

    public async Task<BaseOutput<bool>> SetEnabledAsync(long id, bool enabled)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new(false);
        }

        entity.IsEnabled = enabled;
        await _repository.UpdateAsync(entity, autoSave: true);

        SyncHangfireJob(entity);
        return new(true);
    }

    public async Task<BaseOutput<string>> RunNowAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new("定时计划不存在");
        }

        // 入队后台执行，不阻塞当前请求
        _backgroundJobClient.Enqueue<ITestScheduleJobExecutor>(x => x.ExecuteAsync(id));
        return new("已加入执行队列，执行结果可在执行历史中查看");
    }

    /// <summary>
    /// 按约定生成 recurring job id
    /// </summary>
    private static string RecurringJobId(long scheduleId) => $"{RecurringJobPrefix}{scheduleId}";

    /// <summary>
    /// 同步 Hangfire 周期作业：启用则注册/更新，停用则移除
    /// </summary>
    private void SyncHangfireJob(TestScheduleEntity entity)
    {
        var recurringId = RecurringJobId(entity.Id);

        if (!entity.IsEnabled)
        {
            _recurringJobManager.RemoveIfExists(recurringId);
            return;
        }

        _recurringJobManager.AddOrUpdate<ITestScheduleJobExecutor>(
            recurringId,
            x => x.ExecuteAsync(entity.Id),
            entity.CronExpression);
    }

    private static void ValidateSchedule(TestScheduleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CronExpression))
        {
            throw new UserFriendlyException("Cron 表达式不能为空");
        }

        if (!IsValidCron(dto.CronExpression))
        {
            throw new UserFriendlyException($"Cron 表达式无效：{dto.CronExpression}（示例：0 2 * * * 表示每天 2 点）");
        }

        if (dto.CaseScope != "All" && dto.CaseScope != "Selected")
        {
            throw new UserFriendlyException("用例范围只能是 All 或 Selected");
        }

        if (dto.CaseScope == "Selected" && dto.SelectedCaseIds.Count == 0)
        {
            throw new UserFriendlyException("指定用例范围时必须选择至少一个用例");
        }
    }

    /// <summary>
    /// 校验标准 5 字段 Cron 表达式（支持 *、逗号列表、范围 a-b、步长 /n）
    /// </summary>
    private static bool IsValidCron(string cron)
    {
        var fields = cron.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 5)
        {
            return false;
        }

        (int Min, int Max)[] ranges = [(0, 59), (0, 23), (1, 31), (1, 12), (0, 7)];

        for (var i = 0; i < 5; i++)
        {
            if (!IsValidCronField(fields[i], ranges[i].Min, ranges[i].Max))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidCronField(string field, int min, int max)
    {
        foreach (var part in field.Split(','))
        {
            if (string.IsNullOrEmpty(part))
            {
                return false;
            }

            var rangePart = part;
            var slash = part.IndexOf('/');
            if (slash >= 0)
            {
                rangePart = part[..slash];
                if (!int.TryParse(part[(slash + 1)..], out var step) || step < 1)
                {
                    return false;
                }
            }

            if (rangePart == "*")
            {
                continue;
            }

            var dash = rangePart.IndexOf('-');
            if (dash >= 0)
            {
                if (!int.TryParse(rangePart[..dash], out var from) ||
                    !int.TryParse(rangePart[(dash + 1)..], out var to) ||
                    from < min || to > max || from > to)
                {
                    return false;
                }
            }
            else
            {
                if (!int.TryParse(rangePart, out var value) || value < min || value > max)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static void ApplyDto(TestScheduleEntity entity, TestScheduleDto dto)
    {
        entity.Name = dto.Name;
        entity.EnvId = dto.EnvId;
        entity.CaseScope = dto.CaseScope;
        entity.SelectedCaseIdsJson = dto.CaseScope == "Selected"
            ? JsonSerializer.Serialize(dto.SelectedCaseIds)
            : null;
        entity.CronExpression = dto.CronExpression;
        entity.IsEnabled = dto.IsEnabled;
    }

    private static TestScheduleDto ToDto(TestScheduleEntity entity)
    {
        return new TestScheduleDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            Name = entity.Name,
            EnvId = entity.EnvId,
            CaseScope = entity.CaseScope,
            SelectedCaseIds = string.IsNullOrWhiteSpace(entity.SelectedCaseIdsJson)
                ? new List<long>()
                : JsonSerializer.Deserialize<List<long>>(entity.SelectedCaseIdsJson) ?? new List<long>(),
            CronExpression = entity.CronExpression,
            IsEnabled = entity.IsEnabled,
            LastExecutionTime = entity.LastExecutionTime,
            LastExecutionStatus = entity.LastExecutionStatus
        };
    }
}
