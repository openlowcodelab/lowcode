using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application.Contracts;

/// <summary>
/// 测试执行记录服务接口
/// </summary>
public interface IExecutionRecordAppService : IApplicationService
{
    /// <summary>
    /// 获取指定项目的所有执行记录
    /// </summary>
    Task<List<ExecutionRecordDto>> GetByProjectIdAsync(string projectId);
    
    /// <summary>
    /// 获取指定测试用例的执行记录
    /// </summary>
    Task<List<ExecutionRecordDto>> GetByTestCaseIdAsync(string projectId, string testCaseId);
    
    /// <summary>
    /// 根据ID获取执行记录
    /// </summary>
    Task<ExecutionRecordDto?> GetByIdAsync(string projectId, string id);
    
    /// <summary>
    /// 创建新的执行记录
    /// </summary>
    Task<ExecutionRecordDto> CreateAsync(ExecutionRecordDto record);
    
    /// <summary>
    /// 更新执行记录
    /// </summary>
    Task<bool> UpdateAsync(string projectId, ExecutionRecordDto record);
    
    /// <summary>
    /// 删除执行记录
    /// </summary>
    Task<bool> DeleteAsync(string projectId, string id);
    
    /// <summary>
    /// 清理旧的执行记录（保留最近N条）
    /// </summary>
    Task CleanupOldRecordsAsync(string projectId, int keepCount = 100);
    
    /// <summary>
    /// 获取执行统计信息
    /// </summary>
    Task<ExecutionStatistics> GetStatisticsAsync(string projectId, DateTime? startDate = null, DateTime? endDate = null);
}
