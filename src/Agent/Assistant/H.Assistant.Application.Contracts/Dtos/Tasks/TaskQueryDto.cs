using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 定时任务查询 DTO
/// </summary>
public class TaskQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
    public string? Status { get; set; }
    public bool? IsEnabled { get; set; }

    /// <summary>按任务分类过滤</summary>
    public string? Category { get; set; }
}
