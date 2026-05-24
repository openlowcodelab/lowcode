using Volo.Abp.Domain.Entities.Auditing;

namespace H.Assistant.EntityFrameworkCore;

/// <summary>
/// 定时任务实体
/// </summary>
public class ScheduledTaskEntity : AuditedEntity<Guid>
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务描述
    /// </summary>
    public string TaskDescription { get; set; } = string.Empty;

    /// <summary>
    /// 任务类型（instant/scheduled）
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// 提示词内容
    /// </summary>
    public string PromptContent { get; set; } = string.Empty;

    /// <summary>
    /// Agent类型
    /// </summary>
    public string AgentType { get; set; } = string.Empty;

    /// <summary>
    /// 模型配置ID
    /// </summary>
    public Guid? ModelConfigId { get; set; }

    /// <summary>
    /// 调度类型：Once/Daily/Weekly/Monthly/Cron
    /// </summary>
    public string ScheduleType { get; set; } = string.Empty;

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 执行小时
    /// </summary>
    public int? Hour { get; set; }

    /// <summary>
    /// 执行分钟
    /// </summary>
    public int? Minute { get; set; }

    /// <summary>
    /// 星期几(0-6)
    /// </summary>
    public int? DayOfWeek { get; set; }

    /// <summary>
    /// 每月几号
    /// </summary>
    public int? DayOfMonth { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 最后执行时间
    /// </summary>
    public DateTime? LastExecutionTime { get; set; }

    /// <summary>
    /// 下次执行时间
    /// </summary>
    public DateTime? NextExecutionTime { get; set; }

    /// <summary>
    /// 执行次数
    /// </summary>
    public int ExecutionCount { get; set; }

    /// <summary>
    /// Hangfire任务ID
    /// </summary>
    public string? HangfireJobId { get; set; }

    /// <summary>
    /// 状态：Active/Paused/Completed
    /// </summary>
    public string Status { get; set; } = "Active";
}
