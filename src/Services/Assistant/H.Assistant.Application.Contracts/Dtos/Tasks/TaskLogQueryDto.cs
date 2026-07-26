using H.Abstractions;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 执行日志查询 DTO
/// </summary>
public class TaskLogQueryDto : PagedResultRequestDto
{
    public Guid? TaskId { get; set; }
    public string? Status { get; set; }
}
