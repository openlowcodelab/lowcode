namespace H.Abstractions;

/// <summary>
/// 分页请求 DTO（与 ABP 的 PagedResultRequestDto 保持相同 JSON 序列化结构）
/// </summary>
public class PagedResultRequestDto
{
    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; } = 10;
}
