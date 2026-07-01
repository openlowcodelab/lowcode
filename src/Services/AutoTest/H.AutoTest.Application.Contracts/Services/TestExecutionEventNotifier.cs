using System;
using Volo.Abp.DependencyInjection;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 默认 <see cref="ITestExecutionEventNotifier"/> 实现。
/// 放置在 Contracts 层以便服务端与 Blazor WASM 客户端共享同一实现类型。
/// 服务端通过 ABP 约定注册（ISingletonDependency）；
/// WASM 客户端在 HostAllClientModule 中显式 AddSingleton。
/// </summary>
public class TestExecutionEventNotifier : ITestExecutionEventNotifier, ISingletonDependency
{
    public event Action<string, StepExecutionRecord>? StepUpdated;
    public event Action<string, ExecutionRecordDto>? ExecutionUpdated;

    public void RaiseStepUpdated(string testCaseId, StepExecutionRecord stepRecord)
    {
        StepUpdated?.Invoke(testCaseId, stepRecord);
    }

    public void RaiseExecutionUpdated(string testCaseId, ExecutionRecordDto executionRecord)
    {
        ExecutionUpdated?.Invoke(testCaseId, executionRecord);
    }
}
