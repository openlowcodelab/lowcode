using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 定时任务 DTO
/// </summary>
public class TaskDto : AuditedEntityDto<Guid>
{
    public string TaskName { get; set; } = string.Empty;
    public string TaskDescription { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;

    /// <summary>任务分类</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>创建方式：Prompt(提示词)/Workflow(工作流)</summary>
    public string SourceType { get; set; } = "Prompt";

    /// <summary>工作流步骤（JSON，创建方式为工作流时使用）</summary>
    public string? WorkflowContent { get; set; }

    /// <summary>执行方式：Manual(手动)/Auto(自动)</summary>
    public string ExecutionMode { get; set; } = "Auto";

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

    /// <summary>执行方式显示文本</summary>
    public string ExecutionModeDisplayText => ExecutionMode == "Manual" ? "手动执行" : "自动执行";

    /// <summary>创建方式显示文本</summary>
    public string SourceTypeDisplayText => SourceType == "Workflow" ? "工作流" : "提示词";

    private string GetScheduleDisplay()
    {
        if (ExecutionMode == "Manual")
        {
            return "手动执行";
        }

        return ScheduleType switch
        {
            "Once" => "自动（单次）",
            "Daily" => $"自动（每天 {Hour:D2}:{Minute:D2}）",
            "Weekly" => $"自动（每周 {GetDayOfWeekText()} {Hour:D2}:{Minute:D2}）",
            "Monthly" => $"自动（每月 {DayOfMonth}日 {Hour:D2}:{Minute:D2}）",
            "Cron" => $"自动（Cron: {CronExpression}）",
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
