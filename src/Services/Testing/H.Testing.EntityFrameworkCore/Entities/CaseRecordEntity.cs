using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Testing.EntityFrameworkCore;

/// <summary>
/// 用例执行记录
/// </summary>
public class CaseRecordEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目ID</summary>
    public long ProjectId { get; set; }

    /// <summary>测试用例ID</summary>
    public long TestCaseId { get; set; }

    /// <summary>执行环境ID</summary>
    public long EnvironmentId { get; set; }

    public string? CaseName { get; set; }

    public string? EnvironmentName { get; set; }

    /// <summary>执行状态</summary>
    public int Status { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    /// <summary>总耗时（毫秒）</summary>
    public long Duration { get; set; }

    public int TotalSteps { get; set; }

    public int SuccessSteps { get; set; }

    public int FailedSteps { get; set; }

    public int SkippedSteps { get; set; }

    /// <summary>步骤执行记录（List&lt;StepExecutionRecord&gt; 序列化）</summary>
    public string? StepRecordsJson { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ExecutedBy { get; set; }

    /// <summary>环境配置快照（Dictionary 序列化）</summary>
    public string? EnvironmentSnapshotJson { get; set; }
}
