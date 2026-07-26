using H.BackgroundTask.Application.Contracts;
using H.BackgroundTask.EntityFrameworkCore;

namespace H.BackgroundTask.Application.Mapping;

/// <summary>
/// 实体 &lt;-&gt; DTO 手工映射（与 Order 服务保持一致的轻量映射风格，避免依赖 ABP 的 AutoObjectMappingProvider）。
/// </summary>
public static class BackgroundTaskMappers
{
    // === BackgroundJob ===
    public static BackgroundJobDto ToDto(this BackgroundJobEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        TriggerKind = (JobTriggerKind)e.TriggerKind,
        ExecuteType = (JobExecuteType)e.ExecuteType,
        CronExpression = e.CronExpression,
        ScheduledTime = e.ScheduledTime,
        ApiUrl = e.ApiUrl,
        ApiHttpMethod = e.ApiHttpMethod,
        ApiHeaders = e.ApiHeaders,
        ApiBody = e.ApiBody,
        SqlConnectionString = e.SqlConnectionString,
        SqlStatement = e.SqlStatement,
        IsEnabled = e.IsEnabled,
        HangfireJobId = e.HangfireJobId,
        LastExecutionTime = e.LastExecutionTime,
        LastExecutionStatus = e.LastExecutionStatus.HasValue ? (JobExecutionStatus)e.LastExecutionStatus.Value : null,
        Remark = e.Remark,
        CreationTime = e.CreationTime,
        CreatorId = e.CreatorId,
        LastModificationTime = e.LastModificationTime,
        LastModifierId = e.LastModifierId
    };

    public static BackgroundJobEntity ToEntity(this CreateBackgroundJobDto input) => new()
    {
        Name = input.Name,
        TriggerKind = (int)input.TriggerKind,
        ExecuteType = (int)input.ExecuteType,
        CronExpression = input.CronExpression,
        ScheduledTime = input.ScheduledTime,
        ApiUrl = input.ApiUrl,
        ApiHttpMethod = input.ApiHttpMethod,
        ApiHeaders = input.ApiHeaders,
        ApiBody = input.ApiBody,
        SqlConnectionString = input.SqlConnectionString,
        SqlStatement = input.SqlStatement,
        IsEnabled = input.IsEnabled,
        Remark = input.Remark
    };

    public static void Apply(this UpdateBackgroundJobDto input, BackgroundJobEntity entity)
    {
        entity.Name = input.Name;
        entity.TriggerKind = (int)input.TriggerKind;
        entity.ExecuteType = (int)input.ExecuteType;
        entity.CronExpression = input.CronExpression;
        entity.ScheduledTime = input.ScheduledTime;
        entity.ApiUrl = input.ApiUrl;
        entity.ApiHttpMethod = input.ApiHttpMethod;
        entity.ApiHeaders = input.ApiHeaders;
        entity.ApiBody = input.ApiBody;
        entity.SqlConnectionString = input.SqlConnectionString;
        entity.SqlStatement = input.SqlStatement;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
    }

    // === JobExecutionRecord ===
    public static JobExecutionRecordDto ToDto(this JobExecutionRecordEntity e) => new()
    {
        Id = e.Id,
        JobId = e.JobId,
        JobName = e.JobName,
        ExecuteType = (JobExecuteType)e.ExecuteType,
        Status = (JobExecutionStatus)e.Status,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        DurationMs = e.DurationMs,
        Result = e.Result,
        ErrorMessage = e.ErrorMessage
    };
}
