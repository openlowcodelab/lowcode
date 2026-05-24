using Volo.Abp.Application.Dtos;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 执行日志查询 DTO
/// </summary>
public class TaskExecutionLogQueryDto : PagedResultRequestDto
{
    public Guid? TaskId { get; set; }
    public string? Status { get; set; }
}
