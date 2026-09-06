using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 测试计划
/// </summary>
public class TestPlanEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>计划名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>计划描述</summary>
    public string? Description { get; set; }

    /// <summary>开始日期</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>截止日期</summary>
    public DateTime? DueDate { get; set; }

    /// <summary>状态（对应 TestPlanStatus）</summary>
    public int Status { get; set; }

    /// <summary>执行方式（对应 TestPlanTriggerType，默认手动）</summary>
    public int TriggerType { get; set; }

    /// <summary>定时执行的执行环境ID（TriggerType=Scheduled 时有效）</summary>
    public long EnvId { get; set; }

    /// <summary>Cron 表达式（如 0 2 * * * 表示每天 2 点）</summary>
    public string? CronExpression { get; set; }

    /// <summary>定时执行是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>最近一次定时执行时间</summary>
    public DateTime? LastExecutionTime { get; set; }

    /// <summary>最近一次定时执行状态（对应 ExecutionStatus）</summary>
    public int? LastExecutionStatus { get; set; }
}

/// <summary>
/// 计划-用例关联（计划内的用例及执行进展）
/// </summary>
public class PlanCaseEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public long PlanId { get; set; }

    public long CaseId { get; set; }

    /// <summary>负责人（用户名）</summary>
    public string? Assignee { get; set; }

    /// <summary>计划内状态（对应 PlanCaseStatus）</summary>
    public int Status { get; set; }

    /// <summary>最近一次执行时间</summary>
    public DateTime? LastExecutionTime { get; set; }
}

/// <summary>
/// 缺陷
/// </summary>
public class DefectEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>缺陷标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>缺陷描述</summary>
    public string? Description { get; set; }

    /// <summary>严重程度（对应 DefectSeverity）</summary>
    public int Severity { get; set; }

    /// <summary>状态（对应 DefectStatus）</summary>
    public int Status { get; set; }

    /// <summary>关联用例ID（可空）</summary>
    public long? CaseId { get; set; }

    /// <summary>关联执行记录ID（可空）</summary>
    public long? RecordId { get; set; }

    /// <summary>负责人（用户名）</summary>
    public string? Assignee { get; set; }
}
