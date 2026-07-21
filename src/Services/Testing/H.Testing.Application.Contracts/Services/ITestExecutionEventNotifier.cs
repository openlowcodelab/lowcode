using System;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试执行实时进度事件通知器。
/// 独立于 IApplicationService 以避免 ABP 动态代理对事件 add/remove 进行参数校验。
/// </summary>
public interface ITestExecutionEventNotifier
{
    /// <summary>
    /// 步骤更新事件（testCaseId, stepRecord）。
    /// </summary>
    event Action<long, StepExecutionRecord> StepUpdated;

    /// <summary>
    /// 执行记录更新事件（testCaseId, executionRecord）。
    /// </summary>
    event Action<long, ExecutionRecordDto> ExecutionUpdated;

    /// <summary>
    /// 由 AppService 内部调用以广播步骤更新。
    /// </summary>
    void RaiseStepUpdated(long testCaseId, StepExecutionRecord stepRecord);

    /// <summary>
    /// 由 AppService 内部调用以广播执行记录更新。
    /// </summary>
    void RaiseExecutionUpdated(long testCaseId, ExecutionRecordDto executionRecord);
}
