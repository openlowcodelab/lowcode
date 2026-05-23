using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace H.Agent.Application.Contracts;

/// <summary>
/// 定时任务 DTO
/// </summary>
public class ScheduledTaskDto : AuditedEntityDto<Guid>
{
    public string TaskName { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string PromptContent { get; set; } = string.Empty;
    public string AgentType { get; set; } = string.Empty;
    public Guid? ModelConfigId { get; set; }
    public string ScheduleType { get; set; } = string.Empty;
    public string? CronExpression { get; set; }
    public int? Hour { get; set; }
    public int? Minute { get; set; }
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? LastExecutionTime { get; set; }
    public DateTime? NextExecutionTime { get; set; }
    public int ExecutionCount { get; set; }
    public string? HangfireJobId { get; set; }
    public string Status { get; set; } = string.Empty;

    // 显示用
    public string ScheduleDisplayText => GetScheduleDisplay();

    private string GetScheduleDisplay()
    {
        return ScheduleType switch
        {
            "Once" => "仅一次",
            "Daily" => $"每天 {Hour:D2}:{Minute:D2}",
            "Weekly" => $"每周 {GetDayOfWeekText()} {Hour:D2}:{Minute:D2}",
            "Monthly" => $"每月 {DayOfMonth}日 {Hour:D2}:{Minute:D2}",
            "Cron" => $"Cron: {CronExpression}",
            _ => ScheduleType
        };
    }

    private string GetDayOfWeekText()
    {
        return DayOfWeek switch
        {
            0 => "周日",
            1 => "周一",
            2 => "周二",
            3 => "周三",
            4 => "周四",
            5 => "周五",
            6 => "周六",
            _ => ""
        };
    }
}

/// <summary>
/// 创建定时任务输入 DTO
/// </summary>
public class CreateScheduledTaskInputDto
{
    [Required(ErrorMessage = "任务名称不能为空")]
    [StringLength(100, ErrorMessage = "任务名称不能超过100个字符")]
    public string TaskName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "任务描述不能超过500个字符")]
    public string TaskDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "提示词内容不能为空")]
    public string PromptContent { get; set; } = string.Empty;

    [Required(ErrorMessage = "Agent类型不能为空")]
    public string AgentType { get; set; } = string.Empty;

    public Guid? ModelConfigId { get; set; }

    [Required(ErrorMessage = "调度类型不能为空")]
    public string ScheduleType { get; set; } = string.Empty;

    public string? CronExpression { get; set; }

    public int? Hour { get; set; }

    public int? Minute { get; set; }

    public int? DayOfWeek { get; set; }

    public int? DayOfMonth { get; set; }

    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 更新定时任务输入 DTO
/// </summary>
public class UpdateScheduledTaskInputDto
{
    [Required(ErrorMessage = "任务名称不能为空")]
    [StringLength(100, ErrorMessage = "任务名称不能超过100个字符")]
    public string TaskName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "任务描述不能超过500个字符")]
    public string TaskDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "提示词内容不能为空")]
    public string PromptContent { get; set; } = string.Empty;

    [Required(ErrorMessage = "Agent类型不能为空")]
    public string AgentType { get; set; } = string.Empty;

    public Guid? ModelConfigId { get; set; }

    [Required(ErrorMessage = "调度类型不能为空")]
    public string ScheduleType { get; set; } = string.Empty;

    public string? CronExpression { get; set; }

    public int? Hour { get; set; }

    public int? Minute { get; set; }

    public int? DayOfWeek { get; set; }

    public int? DayOfMonth { get; set; }

    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// 定时任务查询 DTO
/// </summary>
public class ScheduledTaskQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 任务执行日志 DTO
/// </summary>
public class TaskExecutionLogDto : CreationAuditedEntityDto<Guid>
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationSeconds => EndTime.HasValue ? (int)(EndTime.Value - StartTime).TotalSeconds : null;
}

/// <summary>
/// 执行日志查询 DTO
/// </summary>
public class TaskExecutionLogQueryDto : PagedResultRequestDto
{
    public Guid? TaskId { get; set; }
    public string? Status { get; set; }
}
