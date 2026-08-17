using H.Testing.Application.Contracts;
using H.Util.Base;
using System.Collections.Concurrent;
using Volo.Abp.Application.Services;

namespace H.Testing.Application;

/// <summary>
/// 批量执行服务
/// </summary>
public class BatchExecutionAppService : ApplicationService, IBatchExecutionAppService
{
    private readonly ITestExecutionEngineAppService _testExecutionEngine;
    private readonly ICaseAppService _projectCaseService;

    public BatchExecutionAppService(
        ITestExecutionEngineAppService testExecutionEngine,
        ICaseAppService projectCaseService)
    {
        _testExecutionEngine = testExecutionEngine;
        _projectCaseService = projectCaseService;
    }

    /// <summary>
    /// 批量执行测试用例
    /// </summary>
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

            result.TotalTestCases = testCases.Count;

            if (settings.PerformanceSettings.IsPerformanceTestEnabled)
            {
                // 性能测试模式
                await ExecutePerformanceTestAsync(testCases, settings, environmentId, result, cancellationToken);
            }
            else if (settings.IsParallelExecution)
            {
                // 并行执行模式
                await ExecuteParallelAsync(testCases, settings, environmentId, result, cancellationToken);
            }
            else
            {
                // 串行执行模式
                await ExecuteSerialAsync(testCases, settings, environmentId, result, cancellationToken);
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

        return BaseOutput<BatchExecutionResult>.Ok(result);
    }

    /// <summary>
    /// 串行执行测试用例
    /// </summary>
    private async Task ExecuteSerialAsync(
        List<CaseDto> testCases,
        BatchExecutionSettings settings,
        long environmentId,
        BatchExecutionResult result,
        CancellationToken cancellationToken)
    {
        foreach (var testCase in testCases)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var executionRecord = (await _testExecutionEngine.ExecuteTestCaseAsync(
                    testCase, environmentId, cancellationToken)).Data!;

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
            catch (Exception ex)
            {
                result.FailedTestCases++;
                result.Errors.Add($"测试用例 {testCase.Name} 执行失败: {ex.Message}");

                if (!settings.ContinueOnFailure)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 并行执行测试用例
    /// </summary>
    private async Task ExecuteParallelAsync(
        List<CaseDto> testCases,
        BatchExecutionSettings settings,
        long environmentId,
        BatchExecutionResult result,
        CancellationToken cancellationToken)
    {
        var semaphore = new SemaphoreSlim(Environment.ProcessorCount); // 限制并发数
        var tasks = new List<Task>();
        var lockObject = new object();

        foreach (var testCase in testCases)
        {
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var executionRecord = (await _testExecutionEngine.ExecuteTestCaseAsync(
                        testCase, environmentId, cancellationToken)).Data!;

                    lock (lockObject)
                    {
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
                catch (Exception ex)
                {
                    lock (lockObject)
                    {
                        result.FailedTestCases++;
                        result.Errors.Add($"测试用例 {testCase.Name} 执行失败: {ex.Message}");
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
    /// 性能测试执行
    /// </summary>
    private async Task ExecutePerformanceTestAsync(
        List<CaseDto> testCases,
        BatchExecutionSettings settings,
        long environmentId,
        BatchExecutionResult result,
        CancellationToken cancellationToken)
    {
        var performanceSettings = settings.PerformanceSettings;
        var startTime = DateTime.Now;
        var endTime = startTime.AddSeconds(performanceSettings.DurationSeconds);
        var rampUpEndTime = startTime.AddSeconds(performanceSettings.RampUpSeconds);

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
                        testCase, environmentId, cancellationToken)).Data!;

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