using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace H.Testing.Application;

/// <summary>
/// 测试计划定时执行器：由 Hangfire 作业服务器在后台线程调用。
/// 按计划内用例批量执行，并回写计划内用例状态与计划进度。
/// Hangfire 运行在 ABP 环境之外，因此需要手动开启工作单元来读写数据库。
/// </summary>
public class TestPlanScheduleExecutor : ITestPlanScheduleExecutor, ITransientDependency
{
    private readonly IRepository<TestPlanEntity, long> _planRepository;
    private readonly IBatchExecutionAppService _batchExecutionService;
    private readonly ITestPlanAppService _planService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter _dataFilter;
    private readonly ILogger<TestPlanScheduleExecutor> _logger;

    public TestPlanScheduleExecutor(
        IRepository<TestPlanEntity, long> planRepository,
        IBatchExecutionAppService batchExecutionService,
        ITestPlanAppService planService,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentTenant currentTenant,
        IDataFilter dataFilter,
        ILogger<TestPlanScheduleExecutor> logger)
    {
        _planRepository = planRepository;
        _batchExecutionService = batchExecutionService;
        _planService = planService;
        _unitOfWorkManager = unitOfWorkManager;
        _currentTenant = currentTenant;
        _dataFilter = dataFilter;
        _logger = logger;
    }

    public async Task ExecuteAsync(long planId)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);

        TestPlanEntity? plan;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            plan = await _planRepository.FindAsync(planId);
        }

        if (plan is null)
        {
            _logger.LogWarning("测试计划 {PlanId} 不存在，跳过定时执行", planId);
            await uow.CompleteAsync();
            return;
        }

        using (_currentTenant.Change(plan.TenantId))
        {
            await ExecuteCoreAsync(plan);
        }

        await uow.CompleteAsync();
    }

    private async Task ExecuteCoreAsync(TestPlanEntity plan)
    {
        if (plan.TriggerType != (int)TestPlanTriggerType.Scheduled || !plan.IsEnabled)
        {
            return;
        }

        var detail = (await _planService.GetDetailAsync(plan.Id)).Data;
        var caseIds = (detail?.Cases ?? []).Select(c => c.CaseId).ToList();

        var now = DateTime.Now;

        if (caseIds.Count == 0)
        {
            _logger.LogInformation("测试计划 {PlanName} 内无可执行的用例，跳过定时执行", plan.Name);
            plan.LastExecutionTime = now;
            plan.LastExecutionStatus = (int)ExecutionStatus.Cancelled;
            await _planRepository.UpdateAsync(plan);
            return;
        }

        _logger.LogInformation("测试计划 {PlanName} 开始定时执行，共 {CaseCount} 个用例", plan.Name, caseIds.Count);

        var settings = new BatchExecutionSettings
        {
            SelectedTestCaseIds = caseIds,
            EnvironmentId = plan.EnvId,
            IsParallelExecution = false,
            ContinueOnFailure = true
        };

        ExecutionStatus status;
        List<CaseExecutionRecordDto> records = [];
        try
        {
            var result = (await _batchExecutionService.ExecuteBatchAsync(settings, plan.EnvId)).Data;
            status = result?.Status ?? ExecutionStatus.Failed;
            records = result?.ExecutionRecords ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "测试计划 {PlanName} 定时执行失败", plan.Name);
            status = ExecutionStatus.Failed;
        }

        // 回写计划内用例状态并推进计划状态
        await _planService.ApplyResultsAsync(plan.Id, records);

        plan.LastExecutionTime = now;
        plan.LastExecutionStatus = (int)status;
        await _planRepository.UpdateAsync(plan);

        _logger.LogInformation("测试计划 {PlanName} 定时执行完成，状态: {Status}", plan.Name, status);
    }
}
