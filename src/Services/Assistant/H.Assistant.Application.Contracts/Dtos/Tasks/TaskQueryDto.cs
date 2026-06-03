using Volo.Abp.Application.Dtos;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 定时任务查询 DTO
/// </summary>
public class TaskQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public bool? IsEnabled { get; set; }
}
