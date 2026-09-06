using Hangfire;
using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试计划服务：计划增删改查、计划内用例管理、执行结果回写，以及定时执行与 Hangfire 调度联动
/// </summary>
public class TestPlanAppService : ApplicationService, ITestPlanAppService
{
    private const string RecurringJobPrefix = "testing-plan:";

    private readonly IRepository<TestPlanEntity, long> _repository;
    private readonly IRepository<PlanCaseEntity, long> _planCaseRepository;
    private readonly IRepository<CaseEntity, long> _caseRepository;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public TestPlanAppService(
        IRepository<TestPlanEntity, long> repository,
        IRepository<PlanCaseEntity, long> planCaseRepository,
        IRepository<CaseEntity, long> caseRepository,
        IRecurringJobManager recurringJobManager,
        IBackgroundJobClient backgroundJobClient)
    {
        _repository = repository;
        _planCaseRepository = planCaseRepository;
        _caseRepository = caseRepository;
        _recurringJobManager = recurringJobManager;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<BaseOutput<List<TestPlanDto>>> GetByProjectIdAsync(long projectId)
    {
        var planQuery = await _repository.GetQueryableAsync();
        var plans = await AsyncExecuter.ToListAsync(
            planQuery.Where(e => e.ProjectId == projectId).OrderByDescending(e => e.CreationTime));

        var planCaseQuery = await _planCaseRepository.GetQueryableAsync();
        var planIds = plans.Select(p => p.Id).ToList();
        var stats = await AsyncExecuter.ToListAsync(
            planCaseQuery.Where(pc => planIds.Contains(pc.PlanId))
                         .GroupBy(pc => pc.PlanId)
                         .Select(g => new
                         {
                             PlanId = g.Key,
                             Total = g.Count(),
                             Passed = g.Count(pc => pc.Status == (int)PlanCaseStatus.Passed),
                             Failed = g.Count(pc => pc.Status == (int)PlanCaseStatus.Failed)
                         }));
        var statMap = stats.ToDictionary(s => s.PlanId);

        return new(plans.Select(e =>
        {
            var dto = ToDto(e);
            if (statMap.TryGetValue(e.Id, out var stat))
            {
                dto.TotalCases = stat.Total;
                dto.PassedCases = stat.Passed;
                dto.FailedCases = stat.Failed;
            }
            return dto;
        }).ToList());
    }

    public async Task<BaseOutput<TestPlanDetailDto?>> GetDetailAsync(long planId)
    {
        var plan = await _repository.FindAsync(planId);
        if (plan == null)
        {
            return new(null);
        }

        var planCaseQuery = await _planCaseRepository.GetQueryableAsync();
        var planCases = await AsyncExecuter.ToListAsync(planCaseQuery.Where(pc => pc.PlanId == planId));

        var caseQuery = await _caseRepository.GetQueryableAsync();
        var caseIds = planCases.Select(pc => pc.CaseId).ToList();
        var cases = await AsyncExecuter.ToListAsync(caseQuery.Where(c => caseIds.Contains(c.Id)));
        var caseMap = cases.ToDictionary(c => c.Id);

        var detail = new TestPlanDetailDto { Plan = ToDto(plan) };
        detail.Plan.TotalCases = planCases.Count;
        detail.Plan.PassedCases = planCases.Count(pc => pc.Status == (int)PlanCaseStatus.Passed);
        detail.Plan.FailedCases = planCases.Count(pc => pc.Status == (int)PlanCaseStatus.Failed);

        detail.Cases = planCases.Select(pc =>
        {
            caseMap.TryGetValue(pc.CaseId, out var testCase);
            return new PlanCaseDto
            {
                Id = pc.Id,
                PlanId = pc.PlanId,
                CaseId = pc.CaseId,
                CaseName = testCase?.CaseName ?? "(用例已删除)",
                Level = testCase != null ? (CaseLevel)testCase.Level : CaseLevel.P3,
                Assignee = pc.Assignee ?? string.Empty,
                Status = (PlanCaseStatus)pc.Status,
                LastExecutionTime = pc.LastExecutionTime
            };
        }).ToList();

        return new(detail);
    }

    public async Task<BaseOutput<long>> CreateAsync(TestPlanDto dto)
    {
        ValidatePlan(dto);

        var entity = new TestPlanEntity { ProjectId = dto.ProjectId };
        ApplyDto(entity, dto);
        entity = await _repository.InsertAsync(entity, autoSave: true);

        SyncHangfireJob(entity);
        return new(entity.Id);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, TestPlanDto dto)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new(false);
        }

        ValidatePlan(dto);
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

        var planCaseQuery = await _planCaseRepository.GetQueryableAsync();
        var planCases = await AsyncExecuter.ToListAsync(planCaseQuery.Where(pc => pc.PlanId == id));
        await _planCaseRepository.DeleteManyAsync(planCases, autoSave: true);

        await _repository.DeleteAsync(entity, autoSave: true);
        return new(true);
    }

    public async Task<BaseOutput<bool>> SetScheduleEnabledAsync(long id, bool enabled)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null || entity.TriggerType != (int)TestPlanTriggerType.Scheduled)
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
            return new("测试计划不存在");
        }

        // 入队后台执行，不阻塞当前请求
        _backgroundJobClient.Enqueue<ITestPlanScheduleExecutor>(x => x.ExecuteAsync(id));
        return new("已加入执行队列，执行结果可在计划详情中查看");
    }

    public async Task<BaseOutput<int>> AddCasesAsync(long planId, List<long> caseIds)
    {
        var plan = await _repository.FindAsync(planId);
        if (plan == null)
        {
            return new(0);
        }

        var planCaseQuery = await _planCaseRepository.GetQueryableAsync();
        var existingCaseIds = await AsyncExecuter.ToListAsync(
            planCaseQuery.Where(pc => pc.PlanId == planId).Select(pc => pc.CaseId));
        var existingSet = existingCaseIds.ToHashSet();

        var toAdd = caseIds.Distinct()
            .Where(caseId => !existingSet.Contains(caseId))
            .Select(caseId => new PlanCaseEntity
            {
                PlanId = planId,
                CaseId = caseId,
                Status = (int)PlanCaseStatus.NotStarted
            })
            .ToList();

        if (toAdd.Count > 0)
        {
            await _planCaseRepository.InsertManyAsync(toAdd, autoSave: true);
        }

        return new(toAdd.Count);
    }

    public async Task<BaseOutput<bool>> RemoveCaseAsync(long planId, long planCaseId)
    {
        var planCase = await _planCaseRepository.FindAsync(planCaseId);
        if (planCase == null || planCase.PlanId != planId)
        {
            return new(false);
        }

        await _planCaseRepository.DeleteAsync(planCase, autoSave: true);
        return new(true);
    }

    public async Task<BaseOutput<bool>> UpdateCaseAssigneeAsync(long planCaseId, string assignee)
    {
        var planCase = await _planCaseRepository.FindAsync(planCaseId);
        if (planCase == null)
        {
            return new(false);
        }

        planCase.Assignee = string.IsNullOrWhiteSpace(assignee) ? null : assignee.Trim();
        await _planCaseRepository.UpdateAsync(planCase, autoSave: true);
        return new(true);
    }

    public async Task<BaseOutput<bool>> ApplyResultsAsync(long planId, List<CaseExecutionRecordDto> records)
    {
        var plan = await _repository.FindAsync(planId);
        if (plan == null)
        {
            return new(false);
        }

        var planCaseQuery = await _planCaseRepository.GetQueryableAsync();
        var planCases = await AsyncExecuter.ToListAsync(planCaseQuery.Where(pc => pc.PlanId == planId));

        // 每个用例取最新一次执行记录
        var latestByCase = records
            .Where(r => r.EndTime.HasValue)
            .GroupBy(r => r.CaseId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.EndTime).First());

        var changed = false;
        foreach (var planCase in planCases)
        {
            if (!latestByCase.TryGetValue(planCase.CaseId, out var record))
            {
                continue;
            }

            var newStatus = record.Status switch
            {
                ExecutionStatus.Success => PlanCaseStatus.Passed,
                ExecutionStatus.Failed => PlanCaseStatus.Failed,
                _ => (PlanCaseStatus?)null
            };

            if (newStatus.HasValue && planCase.Status != (int)newStatus.Value)
            {
                planCase.Status = (int)newStatus.Value;
                changed = true;
            }

            if (planCase.LastExecutionTime != record.EndTime)
            {
                planCase.LastExecutionTime = record.EndTime;
                changed = true;
            }
        }

        if (changed)
        {
            await _planCaseRepository.UpdateManyAsync(planCases, autoSave: true);
        }

        // 执行后推进计划状态：未开始 → 进行中；全部通过 → 已完成
        if (plan.Status == (int)TestPlanStatus.NotStarted || plan.Status == (int)TestPlanStatus.InProgress)
        {
            var allPassed = planCases.Count > 0 &&
                            planCases.All(pc => pc.Status == (int)PlanCaseStatus.Passed);
            plan.Status = allPassed ? (int)TestPlanStatus.Completed : (int)TestPlanStatus.InProgress;
            await _repository.UpdateAsync(plan, autoSave: true);
        }

        return new(true);
    }

    /// <summary>
    /// 按约定生成 recurring job id
    /// </summary>
    private static string RecurringJobId(long planId) => $"{RecurringJobPrefix}{planId}";

    /// <summary>
    /// 同步 Hangfire 周期作业：定时执行且启用则注册/更新，否则移除
    /// </summary>
    private void SyncHangfireJob(TestPlanEntity entity)
    {
        var recurringId = RecurringJobId(entity.Id);

        if (entity.TriggerType != (int)TestPlanTriggerType.Scheduled || !entity.IsEnabled)
        {
            _recurringJobManager.RemoveIfExists(recurringId);
            return;
        }

        _recurringJobManager.AddOrUpdate<ITestPlanScheduleExecutor>(
            recurringId,
            x => x.ExecuteAsync(entity.Id),
            entity.CronExpression!);
    }

    private static void ValidatePlan(TestPlanDto dto)
    {
        if (dto.TriggerType != TestPlanTriggerType.Scheduled)
        {
            return;
        }

        if (dto.EnvId == 0)
        {
            throw new UserFriendlyException("定时执行必须选择执行环境");
        }

        if (string.IsNullOrWhiteSpace(dto.CronExpression))
        {
            throw new UserFriendlyException("Cron 表达式不能为空");
        }

        if (!IsValidCron(dto.CronExpression))
        {
            throw new UserFriendlyException($"Cron 表达式无效：{dto.CronExpression}（示例：0 2 * * * 表示每天 2 点）");
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

    private static void ApplyDto(TestPlanEntity entity, TestPlanDto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.StartDate = dto.StartDate;
        entity.DueDate = dto.DueDate;
        entity.Status = (int)dto.Status;
        entity.TriggerType = (int)dto.TriggerType;
        entity.EnvId = dto.EnvId;
        entity.CronExpression = dto.TriggerType == TestPlanTriggerType.Scheduled
            ? dto.CronExpression
            : null;
        entity.IsEnabled = dto.TriggerType == TestPlanTriggerType.Scheduled && dto.IsEnabled;
    }

    private static TestPlanDto ToDto(TestPlanEntity entity) => new()
    {
        Id = entity.Id,
        ProjectId = entity.ProjectId,
        Name = entity.Name,
        Description = entity.Description ?? string.Empty,
        StartDate = entity.StartDate,
        DueDate = entity.DueDate,
        Status = (TestPlanStatus)entity.Status,
        TriggerType = (TestPlanTriggerType)entity.TriggerType,
        EnvId = entity.EnvId,
        CronExpression = entity.CronExpression,
        IsEnabled = entity.IsEnabled,
        LastExecutionTime = entity.LastExecutionTime,
        LastExecutionStatus = entity.LastExecutionStatus
    };
}
