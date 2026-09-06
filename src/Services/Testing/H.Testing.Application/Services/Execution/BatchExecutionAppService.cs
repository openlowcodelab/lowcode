using H.Testing.Application.Contracts;
using H.Util.Base;
using System.Collections.Concurrent;
using Volo.Abp.Application.Services;
using Volo.Abp.Uow;

namespace H.Testing.Application;

/// <summary>
/// 批量执行服务：支持串行/并行/性能模式，以及数据驱动与多浏览器执行矩阵
/// </summary>
public class BatchExecutionAppService : ApplicationService, IBatchExecutionAppService
{
    private readonly ITestExecutionEngineAppService _testExecutionEngine;
    private readonly ICaseAppService _projectCaseService;
    private readonly ITestDatasetAppService _datasetService;

    public BatchExecutionAppService(
        ITestExecutionEngineAppService testExecutionEngine,
        ICaseAppService projectCaseService,
        ITestDatasetAppService datasetService)
    {
        _testExecutionEngine = testExecutionEngine;
        _projectCaseService = projectCaseService;
        _datasetService = datasetService;
    }

    /// <summary>
    /// 执行单元：用例 × 浏览器 × 数据行
    /// </summary>
    private sealed record ExecutionUnit(
        CaseDto Case,
        string Browser,
        Dictionary<string, string>? DataVariables,
        string? DataTag);

    /// <summary>
    /// 批量执行测试用例
    /// </summary>
    /// <remarks>
    /// 执行耗时长（UI 用例逐步骤驱动浏览器），不能包在单个数据库事务里：
    /// 非事务 UoW 让每个用例的执行记录在创建时即持久化，即使请求中途被取消，已完成的记录也不会整体回滚
    /// </remarks>
    [UnitOfWork(isTransactional: false)]
    public async Task<BaseOutput<BatchExecutionResult>> ExecuteBatchAsync(
        BatchExecutionSettings settings,
        long environmentId,
        CancellationToken cancellationToken = default)
    {
        var result = new BatchExecutionResult
        {
            BatchId = Guid.NewGuid().ToString(),
            StartTime = DateTime.Now,
            Settings = settings,
            EnvironmentId = environmentId
        };

        try
        {
            // 获取要执行的测试用例
            var testCases = new List<CaseDto>();
            foreach (var testCaseId in settings.SelectedTestCaseIds)
            {
                var testCase = (await _projectCaseService.GetByIdAsync(testCaseId)).Data;
                if (testCase != null)
                {
                    testCases.Add(testCase);
                }
            }

            var browsers = NormalizeBrowsers(settings.Browsers);

            if (settings.PerformanceSettings.IsPerformanceTestEnabled)
            {
                // 性能测试模式（不使用数据集，取第一个浏览器）
                result.TotalTestCases = testCases.Count;
                await ExecutePerformanceTestAsync(testCases, settings, environmentId, result, browsers[0], cancellationToken);
            }
            else
            {
                var dataRows = await LoadDatasetRowsAsync(settings, result);
                var units = BuildUnits(testCases, browsers, dataRows);

                result.TotalTestCases = units.Count;

                if (settings.IsParallelExecution)
                {
                    // 并行执行模式
                    await ExecuteParallelAsync(units, settings, environmentId, result, cancellationToken);
                }
                else
                {
                    // 串行执行模式
                    await ExecuteSerialAsync(units, settings, environmentId, result, cancellationToken);
                }
            }

            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            result.Status = result.FailedTestCases > 0 ? ExecutionStatus.Failed : ExecutionStatus.Success;
        }
        catch (Exception ex)
        {
            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            result.Status = ExecutionStatus.Failed;
            result.ErrorMessage = ex.Message;
        }

        return new(result);
    }

    private static List<string> NormalizeBrowsers(List<string>? browsers)
    {
        var normalized = (browsers ?? [])
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Select(b => b.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        if (normalized.Count == 0)
        {
            normalized.Add("chromium");
        }

        return normalized;
    }

    /// <summary>
    /// 加载数据集的数据行；数据集缺失或为空时记录提示并按无数据集处理
    /// </summary>
    private async Task<List<Dictionary<string, string>>?> LoadDatasetRowsAsync(
        BatchExecutionSettings settings, BatchExecutionResult result)
    {
        if (!settings.DatasetId.HasValue)
        {
            return null;
        }

        var dataset = (await _datasetService.GetByIdAsync(settings.DatasetId.Value)).Data;
        if (dataset == null)
        {
            result.Errors.Add("指定的数据集不存在，将不使用数据集执行");
            return null;
        }

        if (dataset.Rows.Count == 0)
        {
            result.Errors.Add($"数据集 {dataset.Name} 没有数据行，将不使用数据集执行");
            return null;
        }

        return dataset.Rows;
    }

    /// <summary>
    /// 构建执行矩阵：浏览器 × 用例 × 数据行
    /// </summary>
    private static List<ExecutionUnit> BuildUnits(
        List<CaseDto> testCases,
        List<string> browsers,
        List<Dictionary<string, string>>? dataRows)
    {
        var units = new List<ExecutionUnit>();

        foreach (var browser in browsers)
        {
            foreach (var testCase in testCases)
            {
                if (dataRows == null)
                {
                    var tag = browsers.Count > 1 ? browser : null;
                    units.Add(new ExecutionUnit(testCase, browser, null, tag));
                    continue;
                }

                for (var i = 0; i < dataRows.Count; i++)
                {
                    var rowTag = $"数据行 {i + 1}/{dataRows.Count}";
                    var tag = browsers.Count > 1 ? $"{browser} · {rowTag}" : rowTag;
                    units.Add(new ExecutionUnit(testCase, browser, dataRows[i], tag));
                }
            }
        }

        return units;
    }

    /// <summary>
    /// 串行执行执行单元
    /// </summary>
    private async Task ExecuteSerialAsync(
        List<ExecutionUnit> units,
        BatchExecutionSettings settings,
        long environmentId,
        BatchExecutionResult result,
        CancellationToken cancellationToken)
    {
        var lockObject = new object();

        foreach (var unit in units)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var executionRecord = await ExecuteWithRetryAsync(
                unit, settings, environmentId, result, lockObject, cancellationToken);

            if (executionRecord == null)
            {
                result.FailedTestCases++;
                if (!settings.ContinueOnFailure)
                {
                    break;
                }
                continue;
            }

            result.ExecutionRecords.Add(executionRecord);

            if (executionRecord.Status == ExecutionStatus.Success)
            {
                result.SuccessfulTestCases++;
            }
            else
            {
                result.FailedTestCases++;

                // 如果设置为失败时不继续执行，则停止
                if (!settings.ContinueOnFailure)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 并行执行执行单元
    /// </summary>
    private async Task ExecuteParallelAsync(
        List<ExecutionUnit> units,
        BatchExecutionSettings settings,
        long environmentId,
        BatchExecutionResult result,
        CancellationToken cancellationToken)
    {
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount); // 限制并发数
        var tasks = new List<Task>();
        var lockObject = new object();

        foreach (var unit in units)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var executionRecord = await ExecuteWithRetryAsync(
                        unit, settings, environmentId, result, lockObject, cancellationToken);

                    lock (lockObject)
                    {
                        if (executionRecord == null)
                        {
                            result.FailedTestCases++;
                            return;
                        }

                        result.ExecutionRecords.Add(executionRecord);

                        if (executionRecord.Status == ExecutionStatus.Success)
                        {
                            result.SuccessfulTestCases++;
                        }
                        else
                        {
                            result.FailedTestCases++;
                        }
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 执行单个单元并在失败时按配置重试
    /// </summary>
    private async Task<CaseExecutionRecordDto?> ExecuteWithRetryAsync(
        ExecutionUnit unit,
        BatchExecutionSettings settings,
        long environmentId,
        BatchExecutionResult result,
        object lockObject,
        CancellationToken cancellationToken)
    {
        var maxAttempts = Math.Max(settings.RetryCount, 0) + 1;
        CaseExecutionRecordDto? executionRecord = null;

        var options = new ExecutionOptions
        {
            Browser = unit.Browser,
            Headless = settings.Headless,
            DataVariables = unit.DataVariables,
            DataTag = unit.DataTag
        };

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                executionRecord = (await _testExecutionEngine.ExecuteTestCaseAsync(
                    unit.Case, environmentId, cancellationToken, options)).Data;
            }
            catch (Exception ex)
            {
                lock (lockObject)
                {
                    result.Errors.Add($"测试用例 {unit.Case.Name} 执行失败: {ex.Message}");
                }
                executionRecord = null;
            }

            if (executionRecord?.Status == ExecutionStatus.Success || attempt >= maxAttempts)
                break;

            lock (lockObject)
            {
                result.Errors.Add($"测试用例 {unit.Case.Name} 第 {attempt} 次执行失败，即将重试（{attempt}/{settings.RetryCount}）");
            }
        }

        return executionRecord;
    }

    /// <summary>
    /// 性能测试执行
    /// </summary>
    private async Task ExecutePerformanceTestAsync(
        List<CaseDto> testCases,
        BatchExecutionSettings settings,
        long environmentId,
        BatchExecutionResult result,
        string browser,
        CancellationToken cancellationToken)
    {
        var performanceSettings = settings.PerformanceSettings;
        var startTime = DateTime.Now;
        var endTime = startTime.AddSeconds(performanceSettings.DurationSeconds);
        var rampUpEndTime = startTime.AddSeconds(performanceSettings.RampUpSeconds);

        var options = new ExecutionOptions
        {
            Browser = browser,
            Headless = settings.Headless
        };

        var activeTasks = new ConcurrentBag<Task>();
        var semaphore = new SemaphoreSlim(1); // 开始时只有1个并发
        var lockObject = new object();
        var totalExecutions = 0;
        var errorCount = 0;

        // 爬坡阶段：逐渐增加并发数
        var rampUpTask = Task.Run(async () =>
        {
            if (performanceSettings.RampUpSeconds > 0)
            {
                var rampUpInterval = performanceSettings.RampUpSeconds * 1000 / performanceSettings.ConcurrentUsers;
                for (int i = 1; i < performanceSettings.ConcurrentUsers; i++)
                {
                    if (DateTime.Now > rampUpEndTime || cancellationToken.IsCancellationRequested)
                        break;

                    await Task.Delay(rampUpInterval, cancellationToken);
                    semaphore.Release(); // 增加一个并发许可
                }
            }
            else
            {
                // 如果没有爬坡时间，直接释放所有许可
                semaphore.Release(performanceSettings.ConcurrentUsers - 1);
            }
        });

        // 主执行循环
        while (DateTime.Now < endTime && !cancellationToken.IsCancellationRequested)
        {
            // 检查错误率
            lock (lockObject)
            {
                if (totalExecutions > 0)
                {
                    var errorRate = (double)errorCount / totalExecutions * 100;
                    if (errorRate > performanceSettings.MaxErrorRate)
                    {
                        result.Errors.Add($"错误率 {errorRate:F2}% 超过最大允许错误率 {performanceSettings.MaxErrorRate}%，停止测试");
                        break;
                    }
                }
            }

            await semaphore.WaitAsync(cancellationToken);

            var task = Task.Run(async () =>
            {
                try
                {
                    // 随机选择一个测试用例执行
                    var random = new Random();
                    var testCase = testCases[random.Next(testCases.Count)];

                    var executionRecord = (await _testExecutionEngine.ExecuteTestCaseAsync(
                        testCase, environmentId, cancellationToken, options)).Data!;

                    lock (lockObject)
                    {
                        result.ExecutionRecords.Add(executionRecord);
                        totalExecutions++;

                        if (executionRecord.Status == ExecutionStatus.Success)
                        {
                            result.SuccessfulTestCases++;
                        }
                        else
                        {
                            result.FailedTestCases++;
                            errorCount++;
                        }
                    }

                    // 思考时间
                    if (performanceSettings.ThinkTimeMs > 0)
                    {
                        await Task.Delay(performanceSettings.ThinkTimeMs, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    lock (lockObject)
                    {
                        result.FailedTestCases++;
                        errorCount++;
                        totalExecutions++;
                        result.Errors.Add($"性能测试执行失败: {ex.Message}");
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            activeTasks.Add(task);
        }

        // 等待所有任务完成
        await Task.WhenAll(activeTasks);
        await rampUpTask;

        result.TotalExecutions = totalExecutions;
    }
}
