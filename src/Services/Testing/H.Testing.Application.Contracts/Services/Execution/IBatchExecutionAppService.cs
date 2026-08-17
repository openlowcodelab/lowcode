using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 批量执行服务接口
/// </summary>
public interface IBatchExecutionAppService : IAppService
{
    /// <summary>
    /// 批量执行测试用例
    /// </summary>
    Task<BaseOutput<BatchExecutionResult>> ExecuteBatchAsync(
        BatchExecutionSettings settings,
        long environmentId,
        CancellationToken cancellationToken = default);
}
