using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试执行记录服务接口
/// </summary>
public interface IExecutionRecordAppService : IAppService
{
    /// <summary>
    /// 获取指定项目的所有执行记录
    /// </summary>
    Task<BaseOutput<List<CaseExecutionRecordDto>>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 获取指定测试用例的执行记录
    /// </summary>
    Task<BaseOutput<List<CaseExecutionRecordDto>>> GetByTestCaseIdAsync(long projectId, long testCaseId);

    /// <summary>
    /// 根据ID获取执行记录
    /// </summary>
    Task<BaseOutput<CaseExecutionRecordDto?>> GetByIdAsync(long projectId, long id);

    /// <summary>
    /// 创建新的执行记录
    /// </summary>
    Task<BaseOutput<CaseExecutionRecordDto>> CreateAsync(CaseExecutionRecordDto record);

    /// <summary>
    /// 更新执行记录
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long projectId, CaseExecutionRecordDto record);

    /// <summary>
    /// 删除执行记录
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long projectId, long id);

    /// <summary>
    /// 清理旧的执行记录（保留最近N条）
    /// </summary>
    Task<BaseOutput> CleanupOldRecordsAsync(long projectId, int keepCount = 100);

    /// <summary>
    /// 获取执行统计信息
    /// </summary>
    Task<BaseOutput<ExecutionStatistics>> GetStatisticsAsync(long projectId, DateTime? startDate = null, DateTime? endDate = null);
}
