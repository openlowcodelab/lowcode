using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 定时执行计划
/// </summary>
public class TestScheduleEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>计划名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>执行环境ID</summary>
    public long EnvId { get; set; }

    /// <summary>用例范围：All（全部用例）/ Selected（指定用例）</summary>
    public string CaseScope { get; set; } = "All";

    /// <summary>CaseScope=Selected 时的用例ID列表（JSON 数组）</summary>
    public string? SelectedCaseIdsJson { get; set; }

    /// <summary>Cron 表达式（如 0 2 * * * 表示每天 2 点）</summary>
    public string CronExpression { get; set; } = string.Empty;

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>最近一次执行时间</summary>
    public DateTime? LastExecutionTime { get; set; }

    /// <summary>最近一次执行状态（对应 ExecutionStatus）</summary>
    public int? LastExecutionStatus { get; set; }
}
