namespace H.Abstractions;

/// <summary>
/// 分页查询结果 DTO（与 ABP 的 PagedResultDto 保持相同 JSON 序列化结构）
/// </summary>
public class PagedResultDto<T>
{
    public PagedResultDto() { }

    public PagedResultDto(long totalCount, IReadOnlyList<T> items)
    {
        TotalCount = totalCount;
        Items = items;
    }

    public long TotalCount { get; set; }

    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}
