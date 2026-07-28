using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 会话查询 DTO
/// </summary>
public class SessionQueryDto : PagedResultRequestDto
{
    public string? Filter { get; set; }
}
