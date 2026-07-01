using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 批量执行服务接口
/// </summary>
public interface IBatchExecutionAppService : IApplicationService
{
    /// <summary>
    /// 批量执行测试用例
    /// </summary>
    Task<BatchExecutionResult> ExecuteBatchAsync(
        BatchExecutionSettings settings,
        string environmentId,
        CancellationToken cancellationToken = default);
}
