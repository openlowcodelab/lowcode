using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.BackgroundTask.EntityFrameworkCore;

/// <summary>
/// 后台任务定义实体
/// </summary>
public class BackgroundJobEntity : FullAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID（多租户）</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>任务名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>调度方式：0=一次性，1=周期</summary>
    public int TriggerKind { get; set; }

    /// <summary>执行类型：0=API，1=SQL</summary>
    public int ExecuteType { get; set; }

    /// <summary>Cron 表达式（周期任务）</summary>
    public string? CronExpression { get; set; }

    /// <summary>计划执行时间（一次性任务）</summary>
    public DateTime? ScheduledTime { get; set; }

    /// <summary>API 地址</summary>
    public string? ApiUrl { get; set; }

    /// <summary>HTTP 方法</summary>
    public string? ApiHttpMethod { get; set; }

    /// <summary>请求头 JSON</summary>
    public string? ApiHeaders { get; set; }

    /// <summary>请求参数/请求体 JSON</summary>
    public string? ApiBody { get; set; }

    /// <summary>数据源连接字符串</summary>
    public string? SqlConnectionString { get; set; }

    /// <summary>SQL 语句</summary>
    public string? SqlStatement { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Hangfire 作业标识（周期任务为 recurring job id；一次性任务为 background job id）</summary>
    public string? HangfireJobId { get; set; }

    /// <summary>最近一次执行时间</summary>
    public DateTime? LastExecutionTime { get; set; }

    /// <summary>最近一次执行状态：null=未执行，0=成功，1=失败</summary>
    public int? LastExecutionStatus { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
