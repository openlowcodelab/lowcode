using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试计划服务接口
/// </summary>
public interface ITestPlanAppService : IAppService
{
    /// <summary>
    /// 获取项目的测试计划列表（含用例进度统计）
    /// </summary>
    Task<BaseOutput<List<TestPlanDto>>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 获取计划详情（含计划内用例）
    /// </summary>
    Task<BaseOutput<TestPlanDetailDto?>> GetDetailAsync(long planId);

    /// <summary>
    /// 创建计划（TriggerType=Scheduled 时同时注册 Hangfire 周期作业）
    /// </summary>
    Task<BaseOutput<long>> CreateAsync(TestPlanDto dto);

    /// <summary>
    /// 更新计划（同步更新 Hangfire 调度）
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, TestPlanDto dto);

    /// <summary>
    /// 删除计划（同时移除计划内用例与 Hangfire 作业）
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long id);

    /// <summary>
    /// 启用/停用计划的定时执行
    /// </summary>
    Task<BaseOutput<bool>> SetScheduleEnabledAsync(long id, bool enabled);

    /// <summary>
    /// 立即执行一次计划（不影响周期调度）
    /// </summary>
    Task<BaseOutput<string>> RunNowAsync(long id);

    /// <summary>
    /// 向计划添加用例（忽略已存在的）
    /// </summary>
    Task<BaseOutput<int>> AddCasesAsync(long planId, List<long> caseIds);

    /// <summary>
    /// 从计划移除用例
    /// </summary>
    Task<BaseOutput<bool>> RemoveCaseAsync(long planId, long planCaseId);

    /// <summary>
    /// 更新计划内用例的负责人
    /// </summary>
    Task<BaseOutput<bool>> UpdateCaseAssigneeAsync(long planCaseId, string assignee);

    /// <summary>
    /// 回写执行结果：按执行记录更新计划内用例状态，并推进计划状态
    /// </summary>
    Task<BaseOutput<bool>> ApplyResultsAsync(long planId, List<CaseExecutionRecordDto> records);
}
