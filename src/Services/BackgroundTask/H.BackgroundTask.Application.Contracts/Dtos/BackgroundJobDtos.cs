using H.Abp.Application.Contracts;

namespace H.BackgroundTask.Application.Contracts;

/// <summary>
/// 后台任务 DTO
/// </summary>
public class BackgroundJobDto : AuditedEntityDto<Guid>
{
    /// <summary>任务名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>调度方式（一次性/周期）</summary>
    public JobTriggerKind TriggerKind { get; set; }

    /// <summary>执行类型（API/SQL）</summary>
    public JobExecuteType ExecuteType { get; set; }

    /// <summary>Cron 表达式（周期任务）</summary>
    public string? CronExpression { get; set; }

    /// <summary>计划执行时间（一次性任务，为空表示立即执行）</summary>
    public DateTime? ScheduledTime { get; set; }

    /// <summary>API 地址（ExecuteType=Api）</summary>
    public string? ApiUrl { get; set; }

    /// <summary>HTTP 方法（GET/POST/PUT/DELETE）</summary>
    public string? ApiHttpMethod { get; set; }

    /// <summary>请求头（JSON 对象字符串）</summary>
    public string? ApiHeaders { get; set; }

    /// <summary>请求参数/请求体（JSON 字符串）</summary>
    public string? ApiBody { get; set; }

    /// <summary>数据源连接字符串（ExecuteType=Sql）</summary>
    public string? SqlConnectionString { get; set; }

    /// <summary>SQL 语句（ExecuteType=Sql）</summary>
    public string? SqlStatement { get; set; }

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Hangfire 作业标识</summary>
    public string? HangfireJobId { get; set; }

    /// <summary>最近一次执行时间</summary>
    public DateTime? LastExecutionTime { get; set; }

    /// <summary>最近一次执行状态</summary>
    public JobExecutionStatus? LastExecutionStatus { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 创建后台任务 DTO
/// </summary>
public class CreateBackgroundJobDto
{
    public string Name { get; set; } = string.Empty;
    public JobTriggerKind TriggerKind { get; set; }
    public JobExecuteType ExecuteType { get; set; }
    public string? CronExpression { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public string? ApiUrl { get; set; }
    public string? ApiHttpMethod { get; set; } = "GET";
    public string? ApiHeaders { get; set; }
    public string? ApiBody { get; set; }
    public string? SqlConnectionString { get; set; }
    public string? SqlStatement { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? Remark { get; set; }
}

/// <summary>
/// 更新后台任务 DTO
/// </summary>
public class UpdateBackgroundJobDto
{
    public string Name { get; set; } = string.Empty;
    public JobTriggerKind TriggerKind { get; set; }
    public JobExecuteType ExecuteType { get; set; }
    public string? CronExpression { get; set; }
    public DateTime? ScheduledTime { get; set; }
    public string? ApiUrl { get; set; }
    public string? ApiHttpMethod { get; set; }
    public string? ApiHeaders { get; set; }
    public string? ApiBody { get; set; }
    public string? SqlConnectionString { get; set; }
    public string? SqlStatement { get; set; }
    public bool IsEnabled { get; set; }
    public string? Remark { get; set; }
}

/// <summary>
/// 后台任务查询参数
/// </summary>
public class BackgroundJobQueryDto : PagedResultRequestDto
{
    /// <summary>关键词（任务名称）</summary>
    public string? Filter { get; set; }

    /// <summary>调度方式</summary>
    public JobTriggerKind? TriggerKind { get; set; }

    /// <summary>执行类型</summary>
    public JobExecuteType? ExecuteType { get; set; }

    /// <summary>是否启用</summary>
    public bool? IsEnabled { get; set; }
}
