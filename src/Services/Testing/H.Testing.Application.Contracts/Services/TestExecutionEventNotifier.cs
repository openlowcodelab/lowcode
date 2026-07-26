using System;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 默认 <see cref="ITestExecutionEventNotifier"/> 实现。
/// 放置在 Contracts 层以便服务端与 Blazor WASM 客户端共享同一实现类型。
/// 服务端在 TestingApplicationModule 中注册；
/// WASM 客户端在 ClientServices 中显式 AddSingleton。
/// </summary>
public class TestExecutionEventNotifier : ITestExecutionEventNotifier
{
    public event Action<long, StepExecutionRecord>? StepUpdated;
    public event Action<long, ExecutionRecordDto>? ExecutionUpdated;

    public void RaiseStepUpdated(long testCaseId, StepExecutionRecord stepRecord)
    {
        StepUpdated?.Invoke(testCaseId, stepRecord);
    }

    public void RaiseExecutionUpdated(long testCaseId, ExecutionRecordDto executionRecord)
    {
        ExecutionUpdated?.Invoke(testCaseId, executionRecord);
    }
}
