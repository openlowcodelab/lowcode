using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 测试执行引擎服务接口
/// </summary>
public interface ITestExecutionEngineAppService : IApplicationService
{
    /// <summary>
    /// 步骤更新事件
    /// </summary>
    event Action<string, StepExecutionRecord> StepUpdated;
    
    /// <summary>
    /// 执行记录更新事件
    /// </summary>
    event Action<string, ExecutionRecordDto> ExecutionUpdated;
    
    /// <summary>
    /// 执行测试用例
    /// </summary>
    Task<ExecutionRecordDto> ExecuteTestCaseAsync(
        ProjectCaseDto testCase, 
        string environmentId, 
        CancellationToken cancellationToken = default);
}
