using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 定时任务执行日志实体
/// </summary>
public class TaskLogEntity : CreationAuditedEntity<Guid>
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 本次执行的提示词（对话续聊时与任务默认提示词不同，需随日志保存以还原对话）
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// 执行状态：Success/Failed/Cancelled
    /// </summary>
    public string Status { get; set; } = string.Empty;

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
