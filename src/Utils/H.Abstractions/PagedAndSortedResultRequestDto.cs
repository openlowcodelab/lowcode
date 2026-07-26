namespace H.Abstractions;

/// <summary>
/// 分页排序请求 DTO（与 ABP 的 PagedAndSortedResultRequestDto 保持相同 JSON 序列化结构）
/// </summary>
public class PagedAndSortedResultRequestDto : PagedResultRequestDto
{
    public string? Sorting { get; set; }
}
