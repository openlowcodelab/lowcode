using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Testing.Application;

/// <summary>
/// 测试计划服务：计划增删改查、计划内用例管理与执行结果回写
/// </summary>
public class TestPlanAppService : ApplicationService, ITestPlanAppService
{
    private readonly IRepository<TestPlanEntity, long> _repository;
    private readonly IRepository<PlanCaseEntity, long> _planCaseRepository;
    private readonly IRepository<CaseEntity, long> _caseRepository;

    public TestPlanAppService(
        IRepository<TestPlanEntity, long> repository,
        IRepository<PlanCaseEntity, long> planCaseRepository,
        IRepository<CaseEntity, long> caseRepository)
    {
        _repository = repository;
        _planCaseRepository = planCaseRepository;
        _caseRepository = caseRepository;
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
        var entity = new TestPlanEntity { ProjectId = dto.ProjectId };
        ApplyDto(entity, dto);
        entity = await _repository.InsertAsync(entity, autoSave: true);
        return new(entity.Id);
    }

    public async Task<BaseOutput<bool>> UpdateAsync(long id, TestPlanDto dto)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new(false);
        }

        ApplyDto(entity, dto);
        await _repository.UpdateAsync(entity, autoSave: true);
        return new(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(long id)
    {
        var entity = await _repository.FindAsync(id);
        if (entity == null)
        {
            return new(false);
        }

        var planCaseQuery = await _planCaseRepository.GetQueryableAsync();
        var planCases = await AsyncExecuter.ToListAsync(planCaseQuery.Where(pc => pc.PlanId == id));
        await _planCaseRepository.DeleteManyAsync(planCases, autoSave: true);

        await _repository.DeleteAsync(entity, autoSave: true);
        return new(true);
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

    private static void ApplyDto(TestPlanEntity entity, TestPlanDto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.StartDate = dto.StartDate;
        entity.DueDate = dto.DueDate;
        entity.Status = (int)dto.Status;
    }

    private static TestPlanDto ToDto(TestPlanEntity entity) => new()
    {
        Id = entity.Id,
        ProjectId = entity.ProjectId,
        Name = entity.Name,
        Description = entity.Description ?? string.Empty,
        StartDate = entity.StartDate,
        DueDate = entity.DueDate,
        Status = (TestPlanStatus)entity.Status
    };
}
