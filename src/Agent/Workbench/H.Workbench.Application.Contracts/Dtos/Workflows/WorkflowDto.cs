using H.Abp.Application.Contracts;
using System.ComponentModel.DataAnnotations;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 员工工作流 DTO（步骤结构与任务工作流一致，可直接作为任务创建模板）
/// </summary>
public class WorkflowDto : AuditedEntityDto<Guid>
{
    /// <summary>归属员工的 Agent 类型标识</summary>
    public string AgentType { get; set; } = string.Empty;

    public string WorkflowName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    /// <summary>步骤列表（按顺序执行）</summary>
    public List<WorkflowStepDto> Steps { get; set; } = [];

    /// <summary>步骤名串联摘要（列表展示用）</summary>
    public string StepsSummary => string.Join(" → ", Steps.Select(x =>
        string.IsNullOrWhiteSpace(x.Name) ? "未命名步骤" : x.Name.Trim()));
}

public class CreateWorkflowDto
{
    [Required(ErrorMessage = "归属员工不能为空")]
    [StringLength(100, ErrorMessage = "归属员工标识不能超过100个字符")]
    public string AgentType { get; set; } = string.Empty;

    [Required(ErrorMessage = "工作流名称不能为空")]
    [StringLength(200, ErrorMessage = "工作流名称不能超过200个字符")]
    public string WorkflowName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    public List<WorkflowStepDto> Steps { get; set; } = [];
}

public class UpdateWorkflowDto
{
    [Required(ErrorMessage = "工作流名称不能为空")]
    [StringLength(200, ErrorMessage = "工作流名称不能超过200个字符")]
    public string WorkflowName { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "描述不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; } = true;

    /// <summary>步骤列表（保存时全量替换）</summary>
    public List<WorkflowStepDto> Steps { get; set; } = [];
}

public class WorkflowQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }

    /// <summary>按归属员工过滤</summary>
    public string? AgentType { get; set; }

    public bool? IsEnabled { get; set; }
}
