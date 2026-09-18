using H.Abp.Application.Contracts;

namespace H.Workbench.Application.Contracts;

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

    /// <summary>按 Agent 类型过滤</summary>
    public string? AgentType { get; set; }

    /// <summary>按任务类型过滤（instant/scheduled）</summary>
    public string? TaskType { get; set; }

    /// <summary>按归属项目过滤</summary>
    public Guid? ProjectId { get; set; }
}
