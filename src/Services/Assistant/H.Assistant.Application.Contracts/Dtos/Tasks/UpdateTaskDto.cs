using System.ComponentModel.DataAnnotations;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 更新定时任务输入 DTO
/// </summary>
public class UpdateTaskDto
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
