using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace H.Testing.Application;

/// <summary>
/// CI 运行执行器：由 Hangfire 作业服务器在后台线程调用。
/// Hangfire 运行在 ABP 环境之外，因此需要手动开启工作单元来读写数据库。
/// </summary>
public class CiJobExecutor : ICiJobExecutor, ITransientDependency
{
    private readonly IRepository<CiRunEntity, long> _runRepository;
    private readonly ICaseAppService _caseService;
    private readonly IBatchExecutionAppService _batchExecutionService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter _dataFilter;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CiJobExecutor> _logger;

    public CiJobExecutor(
        IRepository<CiRunEntity, long> runRepository,
        ICaseAppService caseService,
        IBatchExecutionAppService batchExecutionService,
        IUnitOfWorkManager unitOfWorkManager,
        ICurrentTenant currentTenant,
        IDataFilter dataFilter,
        IHttpClientFactory httpClientFactory,
        ILogger<CiJobExecutor> logger)
    {
        _runRepository = runRepository;
        _caseService = caseService;
        _batchExecutionService = batchExecutionService;
        _unitOfWorkManager = unitOfWorkManager;
        _currentTenant = currentTenant;
        _dataFilter = dataFilter;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task ExecuteAsync(long runId)
    {
        using var uow = _unitOfWorkManager.Begin(requiresNew: true);

        CiRunEntity? run;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            run = await _runRepository.FindAsync(runId);
        }

        if (run is null)
        {
            _logger.LogWarning("CI 运行 {RunId} 不存在，跳过执行", runId);
            await uow.CompleteAsync();
            return;
        }

        using (_currentTenant.Change(run.TenantId))
        {
            await ExecuteCoreAsync(run);
        }

        await uow.CompleteAsync();
    }

    private async Task ExecuteCoreAsync(CiRunEntity run)
    {
        run.Status = (int)ExecutionStatus.Running;
        run.StartTime = DateTime.Now;
        await _runRepository.UpdateAsync(run, autoSave: true);

        CiRunDto runResult;

        try
        {
            List<long> caseIds;
            if (!string.IsNullOrWhiteSpace(run.CaseIdsJson))
            {
                caseIds = JsonSerializer.Deserialize<List<long>>(run.CaseIdsJson) ?? [];
            }
            else
            {
                caseIds = ((await _caseService.GetByProjectIdAsync(run.ProjectId)).Data ?? [])
                    .Where(c => !c.IsTemplate)
                    .Select(c => c.Id)
                    .ToList();
            }

            if (caseIds.Count == 0)
            {
                run.Status = (int)ExecutionStatus.Failed;
                run.ErrorMessage = "没有可执行的测试用例";
                run.EndTime = DateTime.Now;
                await _runRepository.UpdateAsync(run, autoSave: true);
                return;
            }

            var settings = new BatchExecutionSettings
            {
                SelectedTestCaseIds = caseIds,
                EnvironmentId = run.EnvId,
                IsParallelExecution = false,
                ContinueOnFailure = true,
                Headless = run.Headless,
                Browsers = string.IsNullOrWhiteSpace(run.BrowsersJson)
                    ? new List<string> { "chromium" }
                    : JsonSerializer.Deserialize<List<string>>(run.BrowsersJson) ?? new List<string> { "chromium" }
            };

            var result = (await _batchExecutionService.ExecuteBatchAsync(settings, run.EnvId)).Data;

            run.TotalCases = result?.TotalTestCases ?? caseIds.Count;
            run.SuccessCases = result?.SuccessfulTestCases ?? 0;
            run.FailedCases = result?.FailedTestCases ?? caseIds.Count;
            run.Status = (int)(result?.Status ?? ExecutionStatus.Failed);
            run.ErrorMessage = result is { ErrorMessage.Length: > 0 } ? result.ErrorMessage : null;

            runResult = new CiRunDto
            {
                Id = run.Id,
                ProjectId = run.ProjectId,
                EnvId = run.EnvId,
                Status = (ExecutionStatus)run.Status,
                TotalCases = run.TotalCases,
                SuccessCases = run.SuccessCases,
                FailedCases = run.FailedCases,
                StartTime = run.StartTime,
                EndTime = DateTime.Now,
                CaseResults = (result?.ExecutionRecords ?? []).Select(r => new CiRunCaseResult
                {
                    CaseId = r.CaseId,
                    CaseName = r.CaseName,
                    Status = r.Status,
                    ErrorMessage = r.ErrorMessage,
                    DataTag = r.DataTag
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CI 运行 {RunId} 执行失败", run.Id);
            run.Status = (int)ExecutionStatus.Failed;
            run.ErrorMessage = ex.Message;
            runResult = new CiRunDto
            {
                Id = run.Id,
                ProjectId = run.ProjectId,
                EnvId = run.EnvId,
                Status = ExecutionStatus.Failed,
                ErrorMessage = ex.Message
            };
        }

        run.EndTime = DateTime.Now;
        runResult.EndTime = run.EndTime;
        await _runRepository.UpdateAsync(run, autoSave: true);

        _logger.LogInformation("CI 运行 {RunId} 执行完成，状态: {Status}", run.Id, (ExecutionStatus)run.Status);

        if (!string.IsNullOrWhiteSpace(run.WebhookUrl))
        {
            await NotifyWebhookAsync(run.WebhookUrl, runResult);
        }
    }

    /// <summary>
    /// 执行完成后回调（失败不影响运行结果）
    /// </summary>
    private async Task NotifyWebhookAsync(string webhookUrl, CiRunDto runResult)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            var response = await client.PostAsJsonAsync(webhookUrl, runResult);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CI Webhook 回调返回 {StatusCode}: {Url}", (int)response.StatusCode, webhookUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "CI Webhook 回调失败: {Url}", webhookUrl);
        }
    }
}
