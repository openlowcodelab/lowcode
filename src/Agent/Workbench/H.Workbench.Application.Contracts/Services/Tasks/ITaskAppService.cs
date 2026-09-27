using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 定时任务管理服务接口
/// </summary>
public interface ITaskAppService : IAppService
{
    /// <summary>
    /// 获取任务列表（分页）
    /// </summary>
    Task<BaseOutput<PagedResultDto<TaskDto>>> GetListAsync(TaskQueryDto input);

    /// <summary>
    /// 获取单个任务
    /// </summary>
    Task<BaseOutput<TaskDto>> GetAsync(Guid id);

    /// <summary>
    /// 创建任务
    /// </summary>
    Task<BaseOutput<TaskDto>> CreateAsync(CreateTaskDto input);

    /// <summary>
    /// 更新任务
    /// </summary>
    Task<BaseOutput<TaskDto>> UpdateAsync(Guid id, UpdateTaskDto input);

    /// <summary>
    /// 删除任务
    /// </summary>
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>
    /// 启用/禁用任务
    /// </summary>
    Task<BaseOutput> ToggleEnableAsync(Guid id);

    /// <summary>
    /// 立即执行任务
    /// </summary>
    Task<BaseOutput> ExecuteNowAsync(Guid id);

    /// <summary>
    /// 流式执行任务（SSE 事件负载：thinking/tool_call/tool_result/answer/error）
    /// </summary>
    IAsyncEnumerable<string> ExecuteStreamAsync(ExecuteTaskStreamInputDto input);

    /// <summary>
    /// 执行单个任务（由后台Worker调用）
    /// </summary>
    Task<BaseOutput> ExecuteTaskAsync(Guid taskId);

    /// <summary>
    /// 获取任务的执行日志
    /// </summary>
    Task<BaseOutput<List<TaskLogDto>>> GetExecutionLogsAsync(Guid taskId, int maxResultCount = 10);

    /// <summary>
    /// 获取一次执行的完整轨迹（步骤时间线 + 产物），历史回放用
    /// </summary>
    Task<BaseOutput<TaskLogTraceDto>> GetLogTraceAsync(Guid logId, int maxStepCount = 200);

    /// <summary>
    /// 回传工具审批裁决（立即返回，不等待执行结果；幂等：过期/重复裁决返回失败提示）
    /// </summary>
    Task<BaseOutput<ApprovalOutcomeDto>> ResumeApprovalAsync(ResumeApprovalInputDto input);

    /// <summary>
    /// 人工验收一次执行产出的全部产物，返回被更新的条数
    /// </summary>
    Task<BaseOutput<int>> ReviewArtifactsAsync(ReviewArtifactsInputDto input);

    /// <summary>
    /// 跨任务聚合的审批队列。state 为空时只回待裁决（Pending），否则按指定终态过滤
    /// </summary>
    Task<BaseOutput<List<ApprovalQueueItemDto>>> GetApprovalQueueAsync(string? state = null, int maxCount = 50);

    /// <summary>
    /// 运行看板：按时间窗聚合成功率、验收裁决、token 成本与按员工/任务的分布
    /// </summary>
    Task<BaseOutput<RuntimeStatsDto>> GetRuntimeStatsAsync(int days = 7);

    /// <summary>
    /// 提交一次后台运行，返回 runId（= TaskLog 主键）。浏览器断开不影响执行。
    /// </summary>
    Task<BaseOutput<Guid>> StartRunAsync(StartRunInputDto input);

    /// <summary>
    /// 取消一次仍在排队的运行。已结束的返回失败。
    /// </summary>
    Task<BaseOutput> CancelRunAsync(Guid runId);

    /// <summary>
    /// 运行状态（是否仍在跟踪、当前步数、裁决）
    /// </summary>
    Task<BaseOutput<RunStatusDto>> GetRunStatusAsync(Guid runId);

    /// <summary>
    /// 由执行宿主调用：跑完整个运行并把事件推给 RunEventHub。
    /// 发起人身份不在签名里——它由提交时登记在运行队列中，避免多出一个可伪造 userId
    /// 的 HTTP 入口（审批归属校验要用这个 id）。
    /// </summary>
    Task ExecuteDetachedAsync(Guid taskId, Guid runId, string? prompt, CancellationToken ct);

    /// <summary>
    /// 订阅某次运行的事件流（先回放缓冲再接实时）。断连只是退订。
    /// </summary>
    IAsyncEnumerable<string> SubscribeRunAsync(Guid runId, CancellationToken ct = default);
}
