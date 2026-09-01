using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 定时执行计划服务接口
/// </summary>
public interface ITestScheduleAppService : IAppService
{
    /// <summary>
    /// 获取项目的定时计划列表
    /// </summary>
    Task<BaseOutput<List<TestScheduleDto>>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 创建定时计划（同时注册 Hangfire 周期作业）
    /// </summary>
    Task<BaseOutput<long>> CreateAsync(TestScheduleDto dto);

    /// <summary>
    /// 更新定时计划（同步更新 Hangfire 调度）
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, TestScheduleDto dto);

    /// <summary>
    /// 删除定时计划（同时移除 Hangfire 作业）
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long id);

    /// <summary>
    /// 启用/停用定时计划
    /// </summary>
    Task<BaseOutput<bool>> SetEnabledAsync(long id, bool enabled);

    /// <summary>
    /// 立即执行一次计划（不影响周期调度）
    /// </summary>
    Task<BaseOutput<string>> RunNowAsync(long id);
}
