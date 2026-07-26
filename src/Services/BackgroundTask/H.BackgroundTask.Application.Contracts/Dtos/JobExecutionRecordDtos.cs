using Volo.Abp.Application.Dtos;

namespace H.BackgroundTask.Application.Contracts;

/// <summary>
/// 任务执行记录 DTO
/// </summary>
public class JobExecutionRecordDto : EntityDto<Guid>
{
    /// <summary>任务ID</summary>
    public Guid JobId { get; set; }

    /// <summary>任务名称（冗余，便于列表展示）</summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>执行类型</summary>
    public JobExecuteType ExecuteType { get; set; }

    /// <summary>执行状态</summary>
    public JobExecutionStatus Status { get; set; }

    /// <summary>开始时间</summary>
    public DateTime StartTime { get; set; }

    /// <summary>结束时间</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>耗时（毫秒）</summary>
    public long DurationMs { get; set; }

    /// <summary>执行结果（响应内容 / 影响行数等）</summary>
    public string? Result { get; set; }

    /// <summary>错误信息</summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 任务执行记录查询参数
/// </summary>
public class JobExecutionRecordQueryDto : PagedResultRequestDto
{
    /// <summary>任务ID</summary>
    public Guid? JobId { get; set; }

    /// <summary>执行状态</summary>
    public JobExecutionStatus? Status { get; set; }
}
