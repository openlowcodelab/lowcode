using H.Abp.Application.Contracts;

namespace H.BackgroundTask.Application.Contracts;

/// <summary>
/// 后台任务应用服务接口（对外开放）。
/// ABP 约定控制器将自动生成 RESTful 端点。
/// </summary>
public interface IBackgroundJobAppService : IAppService
{
    /// <summary>分页查询任务</summary>
    Task<PagedResultDto<BackgroundJobDto>> GetListAsync(BackgroundJobQueryDto input);

    /// <summary>按ID获取任务</summary>
    Task<BackgroundJobDto> GetAsync(Guid id);

    /// <summary>创建任务（同时注册到 Hangfire）</summary>
    Task<BackgroundJobDto> CreateAsync(CreateBackgroundJobDto input);

    /// <summary>更新任务（同步更新 Hangfire 调度）</summary>
    Task<BackgroundJobDto> UpdateAsync(Guid id, UpdateBackgroundJobDto input);

    /// <summary>删除任务（同时移除 Hangfire 作业）</summary>
    Task DeleteAsync(Guid id);

    /// <summary>启用任务</summary>
    Task EnableAsync(Guid id);

    /// <summary>禁用任务</summary>
    Task DisableAsync(Guid id);

    /// <summary>手动立即触发一次执行</summary>
    Task TriggerAsync(Guid id);
}
