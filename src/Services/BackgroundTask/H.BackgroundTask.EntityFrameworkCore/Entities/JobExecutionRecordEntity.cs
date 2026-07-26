using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.BackgroundTask.EntityFrameworkCore;

/// <summary>
/// 任务执行记录实体
/// </summary>
public class JobExecutionRecordEntity : CreationAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>任务ID</summary>
    public Guid JobId { get; set; }

    /// <summary>任务名称（冗余）</summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>执行类型：0=API，1=SQL</summary>
    public int ExecuteType { get; set; }

    /// <summary>执行状态：0=成功，1=失败</summary>
    public int Status { get; set; }

    /// <summary>开始时间</summary>
    public DateTime StartTime { get; set; }

    /// <summary>结束时间</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>耗时（毫秒）</summary>
    public long DurationMs { get; set; }

    /// <summary>执行结果</summary>
    public string? Result { get; set; }

    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
}
