using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 定时任务执行日志实体
/// </summary>
public class TaskLogEntity : CreationAuditedEntity<Guid>
{
    public TaskLogEntity()
    {
    }

    /// <summary>
    /// 预分配主键场景：流式执行开始时即确定 logId，供轨迹/产物行挂宿主
    /// </summary>
    public TaskLogEntity(Guid id)
    {
        Id = id;
    }
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 本次执行的提示词（对话续聊时与任务默认提示词不同，需随日志保存以还原对话）
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// 执行状态：Running/Success/Failed/Cancelled/Abandoned
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 本次执行落库的轨迹步骤数
    /// </summary>
    public int StepCount { get; set; }

    /// <summary>
    /// 本次执行产出的产物数
    /// </summary>
    public int ArtifactCount { get; set; }

    /// <summary>
    /// 审批聚合徽标：null（无审批）/Pending/Approved/Denied/Timeout
    /// </summary>
    public string? ApprovalState { get; set; }

    /// <summary>
    /// 执行结果
    /// </summary>
    public string? Result { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
