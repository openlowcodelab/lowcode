using Volo.Abp.Domain.Entities.Auditing;

namespace H.Workbench.EntityFrameworkCore;

/// <summary>
/// 员工工作流实体：归属某个员工的可复用多步流程模板，任务以工作流方式创建时选用
/// </summary>
public class WorkflowEntity : AuditedEntity<Guid>
{
    /// <summary>
    /// 归属员工的 Agent 类型标识
    /// </summary>
    public string AgentType { get; set; } = string.Empty;

    /// <summary>
    /// 工作流名称
    /// </summary>
    public string WorkflowName { get; set; } = string.Empty;

    /// <summary>
    /// 工作流描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 步骤列表（JSON，元素为 WorkflowStepDto）
    /// </summary>
    public string Steps { get; set; } = "[]";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}
