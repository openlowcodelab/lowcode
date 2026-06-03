using Volo.Abp.Application.Dtos;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 任务执行日志 DTO
/// </summary>
public class TaskLogDto : CreationAuditedEntityDto<Guid>
{
    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationSeconds => EndTime.HasValue ? (int)(EndTime.Value - StartTime).TotalSeconds : null;
}
