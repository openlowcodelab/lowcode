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

    /// <summary>任务分类</summary>
    [StringLength(50, ErrorMessage = "任务分类不能超过50个字符")]
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

    [Required(ErrorMessage = "调度类型不能为空")]
    public string ScheduleType { get; set; } = string.Empty;

    public string? CronExpression { get; set; }

    public int? Hour { get; set; }

    public int? Minute { get; set; }

    public int? DayOfWeek { get; set; }

    public int? DayOfMonth { get; set; }

    public bool IsEnabled { get; set; } = true;
}
