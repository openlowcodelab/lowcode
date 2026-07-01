using Volo.Abp.Application.Dtos;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 定时任务 DTO
/// </summary>
public class TaskDto : AuditedEntityDto<Guid>
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
