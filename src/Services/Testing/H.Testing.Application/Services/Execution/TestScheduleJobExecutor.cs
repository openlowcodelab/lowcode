using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace H.Testing.Application;

/// <summary>
/// 定时计划执行器：由 Hangfire 作业服务器在后台线程调用。
/// Hangfire 运行在 ABP 环境之外，因此需要手动开启工作单元来读写数据库。
/// </summary>
public class TestScheduleJobExecutor : ITestScheduleJobExecutor, ITransientDependency
{
    private readonly IRepository<TestScheduleEntity, long> _scheduleRepository;
    private readonly ICaseAppService _caseService;
    private readonly IBatchExecutionAppService _batchExecutionService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter _dataFilter;
    private readonly ILogger<TestScheduleJobExecutor> _logger;

    public TestScheduleJobExecutor(
        IRepository<TestScheduleEntity, long> scheduleRepository,
        ICaseAppService caseService,
        IBatchExecutionAppService batchExecutionService,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentTenant currentTenant,
        IDataFilter dataFilter,
        ILogger<TestScheduleJobExecutor> logger)
    {
        _scheduleRepository = scheduleRepository;
        _caseService = caseService;
        _batchExecutionService = batchExecutionService;
        _unitOfWorkManager = unitOfWorkManager;
        _currentTenant = currentTenant;
        _dataFilter = dataFilter;
        _logger = logger;
    }

    public async Task ExecuteAsync(long scheduleId)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);

        TestScheduleEntity? schedule;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            schedule = await _scheduleRepository.FindAsync(scheduleId);
        }

        if (schedule is null)
        {
            _logger.LogWarning("定时计划 {ScheduleId} 不存在，跳过执行", scheduleId);
            await uow.CompleteAsync();
            return;
        }

        using (_currentTenant.Change(schedule.TenantId))
        {
            await ExecuteCoreAsync(schedule);
        }

        await uow.CompleteAsync();
    }

    private async Task ExecuteCoreAsync(TestScheduleEntity schedule)
    {
        if (!schedule.IsEnabled)
        {
            return;
        }

        List<long> caseIds;
        if (schedule.CaseScope == "Selected" && !string.IsNullOrWhiteSpace(schedule.SelectedCaseIdsJson))
        {
            caseIds = JsonSerializer.Deserialize<List<long>>(schedule.SelectedCaseIdsJson) ?? [];
        }
        else
        {
            caseIds = ((await _caseService.GetByProjectIdAsync(schedule.ProjectId)).Data ?? [])
                .Where(c => !c.IsTemplate)
                .Select(c => c.Id)
                .ToList();
        }

        var now = DateTime.Now;

        if (caseIds.Count == 0)
        {
            _logger.LogInformation("定时计划 {ScheduleName} 无可执行的测试用例，跳过执行", schedule.Name);
            schedule.LastExecutionTime = now;
            schedule.LastExecutionStatus = (int)ExecutionStatus.Cancelled;
            await _scheduleRepository.UpdateAsync(schedule);
            return;
        }

        _logger.LogInformation("定时计划 {ScheduleName} 开始执行，共 {CaseCount} 个用例", schedule.Name, caseIds.Count);

        var settings = new BatchExecutionSettings
        {
            SelectedTestCaseIds = caseIds,
            EnvironmentId = schedule.EnvId,
            IsParallelExecution = false,
            ContinueOnFailure = true
        };

        ExecutionStatus status;
        try
        {
            var result = (await _batchExecutionService.ExecuteBatchAsync(settings, schedule.EnvId)).Data;
            status = result?.Status ?? ExecutionStatus.Failed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "定时计划 {ScheduleName} 执行失败", schedule.Name);
            status = ExecutionStatus.Failed;
        }

        schedule.LastExecutionTime = now;
        schedule.LastExecutionStatus = (int)status;
        await _scheduleRepository.UpdateAsync(schedule);

        _logger.LogInformation("定时计划 {ScheduleName} 执行完成，状态: {Status}", schedule.Name, status);
    }
}
