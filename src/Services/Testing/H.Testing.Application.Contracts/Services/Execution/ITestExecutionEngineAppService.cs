using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试执行引擎服务接口
/// </summary>
/// <remarks>
/// 注意：StepUpdated/ExecutionUpdated 实时进度事件已移至 <see cref="ITestExecutionEventNotifier"/>，
/// 因为 ABP 会为 IApplicationService 生成动态代理并以 ValidationInterceptor 拦截所有方法，
/// 包括 event add/remove，这会对 Action&lt;,&gt; 委托参数递归反射属性，
/// 触发 Type.GenericParameterAttributes 抛 Arg_NotGenericParameter。
/// </remarks>
public interface ITestExecutionEngineAppService : IAppService
{
    /// <summary>
    /// 执行测试用例
    /// </summary>
    Task<BaseOutput<CaseExecutionRecordDto>> ExecuteTestCaseAsync(
        CaseDto testCase,
        long environmentId,
        CancellationToken cancellationToken = default,
        ExecutionOptions? options = null);
}
