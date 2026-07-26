using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试执行记录服务接口
/// </summary>
public interface IExecutionRecordAppService : IApplicationService
{
    /// <summary>
    /// 获取指定项目的所有执行记录
    /// </summary>
    Task<List<ExecutionRecordDto>> GetByProjectIdAsync(long projectId);
    
    /// <summary>
    /// 获取指定测试用例的执行记录
    /// </summary>
    Task<List<ExecutionRecordDto>> GetByTestCaseIdAsync(long projectId, long testCaseId);
    
    /// <summary>
    /// 根据ID获取执行记录
    /// </summary>
    Task<ExecutionRecordDto?> GetByIdAsync(long projectId, long id);
    
    /// <summary>
    /// 创建新的执行记录
    /// </summary>
    Task<ExecutionRecordDto> CreateAsync(ExecutionRecordDto record);
    
    /// <summary>
    /// 更新执行记录
    /// </summary>
    Task<bool> UpdateAsync(long projectId, ExecutionRecordDto record);
    
    /// <summary>
    /// 删除执行记录
    /// </summary>
    Task<bool> DeleteAsync(long projectId, long id);
    
    /// <summary>
    /// 清理旧的执行记录（保留最近N条）
    /// </summary>
    Task CleanupOldRecordsAsync(long projectId, int keepCount = 100);
    
    /// <summary>
    /// 获取执行统计信息
    /// </summary>
    Task<ExecutionStatistics> GetStatisticsAsync(long projectId, DateTime? startDate = null, DateTime? endDate = null);
}
